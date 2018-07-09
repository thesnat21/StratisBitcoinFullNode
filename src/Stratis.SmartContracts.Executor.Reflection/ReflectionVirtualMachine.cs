﻿using System;
using System.Linq;
using System.Reflection;
using Microsoft.Extensions.Logging;
using Stratis.SmartContracts.Core;
using Stratis.SmartContracts.Core.State;
using Stratis.SmartContracts.Executor.Reflection.Exceptions;
using Stratis.SmartContracts.Executor.Reflection.Lifecycle;

namespace Stratis.SmartContracts.Executor.Reflection
{
    /// <summary>
    /// Used to instantiate smart contracts using reflection and then execute certain methods and their parameters.
    /// </summary>
    public class ReflectionVirtualMachine : ISmartContractVirtualMachine
    {
        private readonly InternalTransactionExecutorFactory internalTransactionExecutorFactory;
        private readonly ILogger logger;
        public static int VmVersion = 1;

        public ReflectionVirtualMachine(
            InternalTransactionExecutorFactory internalTransactionExecutorFactory,
            ILoggerFactory loggerFactory)
        {
            this.internalTransactionExecutorFactory = internalTransactionExecutorFactory;
            this.logger = loggerFactory.CreateLogger(this.GetType());
        }

        /// <summary>
        /// Creates a new instance of a smart contract by invoking the contract's constructor
        /// </summary>
        public ISmartContractExecutionResult Create(byte[] contractCode,
            ISmartContractExecutionContext context,
            IGasMeter gasMeter,
            IPersistentState persistentState, 
            IContractStateRepository repository)
        {
            this.logger.LogTrace("()");

            byte[] gasInjectedCode = SmartContractGasInjector.AddGasCalculationToConstructor(contractCode);

            Type contractType = Load(gasInjectedCode);

            var internalTransferList = new InternalTransferList();

            IInternalTransactionExecutor internalTransactionExecutor = this.internalTransactionExecutorFactory.Create(repository, internalTransferList);

            var balanceState = new BalanceState(repository, context.Message.Value, internalTransferList);

            var contractState = new SmartContractState(
                context.Block,
                context.Message,
                persistentState,
                gasMeter,
                internalTransactionExecutor,
                new InternalHashHelper(),
                () => balanceState.GetBalance(context.ContractAddress));

            // Invoke the constructor of the provided contract code
            LifecycleResult result = SmartContractConstructor.Construct(contractType, contractState, context.Parameters);

            ISmartContractExecutionResult executionResult = new SmartContractExecutionResult
            {
                GasConsumed = gasMeter.GasConsumed
            };

            if (!result.Success)
            {
                LogException(result.Exception);

                this.logger.LogTrace("(-)[CREATE_CONTRACT_INSTANTIATION_FAILED]:{0}={1}", nameof(gasMeter.GasConsumed), gasMeter.GasConsumed);

                executionResult.Exception = result.Exception.InnerException ?? result.Exception;
                return executionResult;
            }
            else
                this.logger.LogTrace("[CREATE_CONTRACT_INSTANTIATION_SUCCEEDED]");

            executionResult.Return = result.Object;

            this.logger.LogTrace("(-):{0}={1}", nameof(gasMeter.GasConsumed), gasMeter.GasConsumed);

            return executionResult;
        }

        /// <summary>
        /// Invokes a method on an existing smart contract
        /// </summary>
        public ISmartContractExecutionResult ExecuteMethod(byte[] contractCode,
            string contractMethodName,
            ISmartContractExecutionContext context,
            IGasMeter gasMeter,
            IPersistentState persistentState, 
            IContractStateRepository repository)
        {
            this.logger.LogTrace("(){0}:{1}", nameof(contractMethodName), contractMethodName);

            ISmartContractExecutionResult executionResult = new SmartContractExecutionResult();

            if (contractMethodName == null)
            {
                this.logger.LogTrace("(-)[CALLCONTRACT_METHODNAME_NOT_GIVEN]");
                return executionResult;
            }

            byte[] gasInjectedCode = SmartContractGasInjector.AddGasCalculationToContractMethod(contractCode, contractMethodName);
            Type contractType = Load(gasInjectedCode);
            if (contractType == null)
            {
                this.logger.LogTrace("(-)[CALLCONTRACT_CONTRACTTYPE_NULL]");
                return executionResult;
            }

            var internalTransferList = new InternalTransferList();

            IInternalTransactionExecutor internalTransactionExecutor = this.internalTransactionExecutorFactory.Create(repository, internalTransferList);

            var balanceState = new BalanceState(repository, context.Message.Value, internalTransferList);

            var contractState = new SmartContractState(
                context.Block,
                context.Message,
                persistentState,
                gasMeter,
                internalTransactionExecutor,
                new InternalHashHelper(),
                () => balanceState.GetBalance(context.ContractAddress));

            LifecycleResult result = SmartContractRestorer.Restore(contractType, contractState);

            if (!result.Success)
            {
                LogException(result.Exception);

                this.logger.LogTrace("(-)[CALLCONTRACT_INSTANTIATION_FAILED]:{0}={1}", nameof(gasMeter.GasConsumed), gasMeter.GasConsumed);

                executionResult.Exception = result.Exception.InnerException ?? result.Exception;
                executionResult.GasConsumed = gasMeter.GasConsumed;

                return executionResult;
            }
            else
                this.logger.LogTrace("[CALL_CONTRACT_INSTANTIATION_SUCCEEDED]");

            try
            {
                MethodInfo methodToInvoke = contractType.GetMethod(contractMethodName);
                if (methodToInvoke == null)
                    throw new ArgumentException(string.Format("[CALLCONTRACT_METHODTOINVOKE_NULL_DOESNOT_EXIST]:{0}={1}", nameof(contractMethodName), contractMethodName));

                if (methodToInvoke.IsConstructor)
                    throw new ConstructorInvocationException("[CALLCONTRACT_CANNOT_INVOKE_CTOR]");

                SmartContract smartContract = result.Object;
                executionResult.Return = methodToInvoke.Invoke(smartContract, context.Parameters);

                executionResult.InternalTransfers = internalTransferList.Transfers;
            }
            catch (ArgumentException argumentException)
            {
                LogException(argumentException);
                executionResult.Exception = argumentException;
            }
            catch (TargetInvocationException targetException)
            {
                LogException(targetException);
                executionResult.Exception = targetException.InnerException ?? targetException;
            }
            catch (TargetParameterCountException parameterExcepion)
            {
                LogException(parameterExcepion);
                executionResult.Exception = parameterExcepion;
            }
            catch (ConstructorInvocationException constructorInvocationException)
            {
                LogException(constructorInvocationException);
                executionResult.Exception = constructorInvocationException;
            }
            finally
            {
                executionResult.GasConsumed = gasMeter.GasConsumed;
            }

            this.logger.LogTrace("(-):{0}={1}", nameof(gasMeter.GasConsumed), gasMeter.GasConsumed);

            return executionResult;
        }

        private void LogException(Exception exception)
        {
            this.logger.LogTrace("{0}", exception.Message);
            if (exception.InnerException != null)
                this.logger.LogTrace("{0}", exception.InnerException.Message);
        }

        /// <summary>
        /// Loads the Assembly bytecode into the current AppDomain.
        /// <para>
        /// The contract should always be the only exported type.
        /// </para>
        /// </summary>
        private static Type Load(byte[] byteCode)
        {
            Assembly contractAssembly = Assembly.Load(byteCode);
            return contractAssembly.ExportedTypes.FirstOrDefault();
        }
    }
}