﻿using System.Collections.Generic;
using System.Linq;
using Mono.Cecil;
using Stratis.Validators.Net;

namespace Stratis.SmartContracts.Core.ContractValidation
{
    /// <summary>
    /// Validates that a <see cref="Mono.Cecil.MethodDefinition"/> does not reference a method with a known forbidden return type
    /// </summary>
    public class ReferencedMethodReturnTypeValidator : IMethodDefinitionValidator
    {
        public static readonly string ErrorType = "Invalid Return Type in Referenced Method";

        public static readonly HashSet<string> RedLightMethods = new HashSet<string>
        {
            "System.IRuntimeMethodInfo System.Exception::GetMethodFromStackTrace(System.Object)",
            "System.DateTime System.DateTime::Add(System.Double,System.Int32)",
            "System.DateTime System.DateTime::Add(System.Double,System.Int32)",
            "System.DateTime System.DateTime::Add(System.Double,System.Int32)",
            "System.DateTime System.DateTime::Add(System.Double,System.Int32)",
            "System.Double System.DateTime::TicksToOADate(System.Int64)",
            "System.Double System.DateTime::TicksToOADate(System.Int64)",
            "System.Double System.DateTime::TicksToOADate(System.Int64)",
            "System.MulticastDelegate System.Delegate::InternalAlloc(System.RuntimeType)",
            "System.MulticastDelegate System.Delegate::InternalAllocLike(System.Delegate)",
            "System.IRuntimeMethodInfo System.Delegate::FindMethodHandle()",
            "System.AppDomain/APPX_FLAGS System.AppDomain::get_Flags()",
            "System.AppDomain/APPX_FLAGS System.AppDomain::nGetAppXFlags()",
            "System.AppDomain/APPX_FLAGS System.AppDomain::nGetAppXFlags()",
            "System.AppDomainHandle System.AppDomain::GetNativeHandle()",
            "System.AppDomainManager System.AppDomain::get_DomainManager()",
            "System.AppDomain System.AppDomain::get_CurrentDomain()",
            "System.Reflection.Assembly[] System.AppDomain::nGetAssemblies(System.Boolean)",
            "System.Reflection.Assembly[] System.AppDomain::nGetAssemblies(System.Boolean)",
            "System.Reflection.Assembly[] System.AppDomain::GetAssemblies(System.Boolean)",
            "System.Reflection.RuntimeAssembly System.AppDomain::OnResourceResolveEvent(System.Reflection.RuntimeAssembly,System.String)",
            "System.Reflection.RuntimeAssembly System.AppDomain::OnTypeResolveEvent(System.Reflection.RuntimeAssembly,System.String)",
            "System.Reflection.RuntimeAssembly System.AppDomain::OnAssemblyResolveEvent(System.Reflection.RuntimeAssembly,System.String)",
            "System.AppDomainSetup System.AppDomain::get_FusionStore()",
            "System.Reflection.RuntimeAssembly System.AppDomain::GetRuntimeAssembly(System.Reflection.Assembly)",
            "System.AppDomainSetup System.AppDomain::get_SetupInformation()",
            "System.Double System.BitConverter::ToDouble(System.Byte[],System.Int32)",
            "System.Double System.BitConverter::Int64BitsToDouble(System.Int64)",
            "System.Double System.Decimal::ToDouble(System.Decimal)",
            "System.Double System.Double::System.IConvertible.ToDouble(System.IFormatProvider)",
            "System.RuntimeType System.Enum::InternalGetUnderlyingType(System.RuntimeType)",
            "System.Reflection.CorElementType System.Enum::InternalGetCorElementType()",
            "System.Double System.Math::Acos(System.Double)",
            "System.Double System.Math::Asin(System.Double)",
            "System.Double System.Math::Atan(System.Double)",
            "System.Double System.Math::Atan2(System.Double,System.Double)",
            "System.Double System.Math::Ceiling(System.Double)",
            "System.Double System.Math::Cos(System.Double)",
            "System.Double System.Math::Cosh(System.Double)",
            "System.Double System.Math::Floor(System.Double)",
            "System.Double System.Math::InternalRound(System.Double,System.Int32,System.MidpointRounding)",
            "System.Double System.Math::InternalRound(System.Double,System.Int32,System.MidpointRounding)",
            "System.Double System.Math::InternalRound(System.Double,System.Int32,System.MidpointRounding)",
            "System.Double System.Math::Sin(System.Double)",
            "System.Double System.Math::Tan(System.Double)",
            "System.Double System.Math::Sinh(System.Double)",
            "System.Double System.Math::Tanh(System.Double)",
            "System.Double System.Math::Round(System.Double)",
            "System.Double System.Math::SplitFractionDouble(System.Double*)",
            "System.Double System.Math::Sqrt(System.Double)",
            "System.Double System.Math::Log(System.Double)",
            "System.Double System.Math::Log10(System.Double)",
            "System.Double System.Math::Exp(System.Double)",
            "System.Double System.Math::Pow(System.Double,System.Double)",
            "System.Double System.Math::IEEERemainder(System.Double,System.Double)",
            "System.Double System.Math::IEEERemainder(System.Double,System.Double)",
            "System.Double System.Math::IEEERemainder(System.Double,System.Double)",
            "System.Double System.Math::Abs(System.Double)",
            "System.Double System.Math::Log(System.Double,System.Double)",
            "System.Double System.Math::Log(System.Double,System.Double)",
            "System.Double System.Math::Log(System.Double,System.Double)",
            "System.Double System.Math::Log(System.Double,System.Double)",
            "System.Double System.Math::Log(System.Double,System.Double)",
            "System.Double System.Number::ParseDouble(System.String,System.Globalization.NumberStyles,System.Globalization.NumberFormatInfo)",
            "System.Double System.Number::ParseDouble(System.String,System.Globalization.NumberStyles,System.Globalization.NumberFormatInfo)",
            "System.Double System.Number::ParseDouble(System.String,System.Globalization.NumberStyles,System.Globalization.NumberFormatInfo)",
            "System.Double System.Number::ParseDouble(System.String,System.Globalization.NumberStyles,System.Globalization.NumberFormatInfo)",
            "System.Reflection.CorElementType System.RuntimeTypeHandle::GetCorElementType(System.RuntimeType)",
            "System.Reflection.RuntimeAssembly System.RuntimeTypeHandle::GetAssembly(System.RuntimeType)",
            "System.Reflection.RuntimeModule System.RuntimeTypeHandle::GetModule(System.RuntimeType)",
            "System.RuntimeType System.RuntimeTypeHandle::GetBaseType(System.RuntimeType)",
            "System.Reflection.TypeAttributes System.RuntimeTypeHandle::GetAttributes(System.RuntimeType)",
            "System.RuntimeType System.RuntimeTypeHandle::GetElementType(System.RuntimeType)",
            "System.RuntimeMethodHandleInternal System.RuntimeTypeHandle::GetMethodAt(System.RuntimeType,System.Int32)",
            "System.RuntimeMethodHandleInternal System.RuntimeTypeHandle::GetFirstIntroducedMethod(System.RuntimeType)",
            "System.RuntimeType System.RuntimeTypeHandle::GetDeclaringType(System.RuntimeType)",
            "System.IRuntimeMethodInfo System.RuntimeTypeHandle::GetDeclaringMethod(System.RuntimeType)",
            "System.IRuntimeMethodInfo System.RuntimeMethodHandle::_GetCurrentMethod(System.Threading.StackCrawlMark&)",
            "System.Reflection.MethodAttributes System.RuntimeMethodHandle::GetAttributes(System.RuntimeMethodHandleInternal)",
            "System.Reflection.MethodImplAttributes System.RuntimeMethodHandle::GetImplAttributes(System.IRuntimeMethodInfo)",
            "System.RuntimeType System.RuntimeMethodHandle::GetDeclaringType(System.RuntimeMethodHandleInternal)",
            "System.RuntimeMethodHandleInternal System.RuntimeMethodHandle::GetStubIfNeeded(System.RuntimeMethodHandleInternal,System.RuntimeType,System.RuntimeType[])",
            "System.RuntimeMethodHandleInternal System.RuntimeMethodHandle::GetMethodFromCanonical(System.RuntimeMethodHandleInternal,System.RuntimeType)",
            "System.Resolver System.RuntimeMethodHandle::GetResolver(System.RuntimeMethodHandleInternal)",
            "System.Reflection.MethodBody System.RuntimeMethodHandle::GetMethodBody(System.IRuntimeMethodInfo,System.RuntimeType)",
            "System.Reflection.LoaderAllocator System.RuntimeMethodHandle::GetLoaderAllocator(System.RuntimeMethodHandleInternal)",
            "System.Reflection.FieldAttributes System.RuntimeFieldHandle::GetAttributes(System.RuntimeFieldHandleInternal)",
            "System.RuntimeType System.RuntimeFieldHandle::GetApproxDeclaringType(System.RuntimeFieldHandleInternal)",
            "System.RuntimeFieldHandleInternal System.RuntimeFieldHandle::GetStaticFieldForGenericType(System.RuntimeFieldHandleInternal,System.RuntimeType)",
            "System.IRuntimeMethodInfo System.ModuleHandle::GetDynamicMethod(System.Reflection.Emit.DynamicMethod,System.Reflection.RuntimeModule,System.String,System.Byte[],System.Resolver)",
            "System.RuntimeMethodHandleInternal System.ModuleHandle::ResolveMethod(System.Reflection.RuntimeModule,System.Int32,System.IntPtr*,System.Int32,System.IntPtr*,System.Int32)",
            "System.Double System.Single::System.IConvertible.ToDouble(System.IFormatProvider)",
            "System.Double System.TimeSpan::get_TotalDays()",
            "System.Double System.TimeSpan::get_TotalDays()",
            "System.Double System.TimeSpan::get_TotalHours()",
            "System.Double System.TimeSpan::get_TotalHours()",
            "System.Double System.TimeSpan::get_TotalMilliseconds()",
            "System.Double System.TimeSpan::get_TotalMilliseconds()",
            "System.Double System.TimeSpan::get_TotalMilliseconds()",
            "System.Double System.TimeSpan::get_TotalMilliseconds()",
            "System.Double System.TimeSpan::get_TotalMilliseconds()",
            "System.Double System.TimeSpan::get_TotalMilliseconds()",
            "System.Double System.TimeSpan::get_TotalMinutes()",
            "System.Double System.TimeSpan::get_TotalMinutes()",
            "System.Double System.TimeSpan::get_TotalSeconds()",
            "System.Double System.TimeSpan::get_TotalSeconds()",
            "System.TimeSpan System.TimeSpan::Interval(System.Double,System.Int32)",
            "System.TimeSpan System.TimeSpan::Interval(System.Double,System.Int32)",
            "System.TimeSpan System.TimeSpan::Interval(System.Double,System.Int32)",
            "System.TimeSpan System.TimeSpan::Interval(System.Double,System.Int32)",
            "System.TimeSpan System.TimeSpan::Interval(System.Double,System.Int32)",
            "System.TimeSpan System.TimeSpan::Interval(System.Double,System.Int32)",
            "System.TimeSpan System.TimeSpan::op_Multiply(System.TimeSpan,System.Double)",
            "System.TimeSpan System.TimeSpan::op_Multiply(System.TimeSpan,System.Double)",
            "System.TimeSpan System.TimeSpan::op_Multiply(System.TimeSpan,System.Double)",
            "System.TimeSpan System.TimeSpan::op_Division(System.TimeSpan,System.Double)",
            "System.TimeSpan System.TimeSpan::op_Division(System.TimeSpan,System.Double)",
            "System.TimeSpan System.TimeSpan::op_Division(System.TimeSpan,System.Double)",
            "System.Double System.TimeSpan::op_Division(System.TimeSpan,System.TimeSpan)",
            "System.Double System.TimeSpan::op_Division(System.TimeSpan,System.TimeSpan)",
            "System.DateTime System.TimeZoneInfo::TransitionTimeToDateTime(System.Int32,System.TimeZoneInfo/TransitionTime)",
            "System.DateTime System.TimeZoneInfo::TransitionTimeToDateTime(System.Int32,System.TimeZoneInfo/TransitionTime)",
            "System.TimeZoneInfo/AdjustmentRule System.TimeZoneInfo::CreateAdjustmentRuleFromTimeZoneInformation(Microsoft.Win32.Win32Native/RegistryTimeZoneInformation,System.DateTime,System.DateTime,System.Int32)",
            "System.RuntimeType System.Type::GetTypeFromHandleUnsafe(System.IntPtr)",
            "T System.WeakReference`1::get_Target()",
            "System.Double System.Variant::GetR8FromVar()",
            "System.Double System.Convert::ToDouble(System.Object)",
            "System.Double System.Convert::ToDouble(System.Object,System.IFormatProvider)",
            "System.Double System.Convert::ToDouble(System.SByte)",
            "System.Double System.Convert::ToDouble(System.Byte)",
            "System.Double System.Convert::ToDouble(System.Int16)",
            "System.Double System.Convert::ToDouble(System.UInt16)",
            "System.Double System.Convert::ToDouble(System.Int32)",
            "System.Double System.Convert::ToDouble(System.UInt32)",
            "System.Double System.Convert::ToDouble(System.UInt32)",
            "System.Double System.Convert::ToDouble(System.Int64)",
            "System.Double System.Convert::ToDouble(System.UInt64)",
            "System.Double System.Convert::ToDouble(System.UInt64)",
            "System.Double System.Convert::ToDouble(System.Single)",
            "System.Double System.Convert::ToDouble(System.Decimal)",
            "System.Double System.Convert::ToDouble(System.String)",
            "System.Double System.Convert::ToDouble(System.String,System.IFormatProvider)",
            "System.Double System.Convert::ToDouble(System.Boolean)",
            "System.Double System.Random::Sample()",
            "System.Double System.Random::Sample()",
            "System.Double System.Random::GetSampleForLargeRange()",
            "System.Double System.Random::GetSampleForLargeRange()",
            "System.Double System.Random::GetSampleForLargeRange()",
            "System.Double System.IO.BinaryReader::ReadDouble()",
            "System.Double System.Threading.Interlocked::Exchange(System.Double&,System.Double)",
            "System.Double System.Threading.Interlocked::CompareExchange(System.Double&,System.Double,System.Double)",
            "System.Threading.NativeOverlapped* System.Threading.OverlappedData::AllocateNativeOverlapped()",
            "System.Threading.OverlappedData System.Threading.OverlappedData::GetOverlappedFromNative(System.Threading.NativeOverlapped*)",
            "System.Threading.Thread System.Threading.Thread::GetCurrentThreadNative()",
            "System.AppDomain System.Threading.Thread::GetDomainInternal()",
            "System.AppDomain System.Threading.Thread::GetFastDomainInternal()",
            "System.Threading.TimerQueue/AppDomainTimerSafeHandle System.Threading.TimerQueue::CreateAppDomainTimer(System.UInt32)",
            "System.Double System.Threading.Volatile::Read(System.Double&)",
            "System.Double System.Threading.Volatile::Read(System.Double&)",
            "System.DateTime System.Globalization.Calendar::Add(System.DateTime,System.Double,System.Int32)",
            "System.DateTime System.Globalization.Calendar::Add(System.DateTime,System.Double,System.Int32)",
            "System.DateTime System.Globalization.Calendar::Add(System.DateTime,System.Double,System.Int32)",
            "System.DateTime System.Globalization.Calendar::Add(System.DateTime,System.Double,System.Int32)",
            "System.DateTime System.Globalization.Calendar::Add(System.DateTime,System.Double,System.Int32)",
            "System.DateTime System.Globalization.Calendar::Add(System.DateTime,System.Double,System.Int32)",
            "System.DateTime System.Globalization.Calendar::AddDays(System.DateTime,System.Int32)",
            "System.DateTime System.Globalization.Calendar::AddHours(System.DateTime,System.Int32)",
            "System.DateTime System.Globalization.Calendar::AddMinutes(System.DateTime,System.Int32)",
            "System.DateTime System.Globalization.Calendar::AddSeconds(System.DateTime,System.Int32)",
            "System.Double System.Globalization.Calendar::GetMilliseconds(System.DateTime)",
            "System.Double System.Globalization.CharUnicodeInfo::InternalGetNumericValue(System.Int32)",
            "System.Globalization.InternalEncodingDataItem* System.Globalization.EncodingTable::GetEncodingData()",
            "System.Globalization.InternalCodePageDataItem* System.Globalization.EncodingTable::GetCodePageData()",
            "System.Double System.Globalization.CalendricalCalculationsHelper::RadiansFromDegrees(System.Double)",
            "System.Double System.Globalization.CalendricalCalculationsHelper::RadiansFromDegrees(System.Double)",
            "System.Double System.Globalization.CalendricalCalculationsHelper::Angle(System.Int32,System.Int32,System.Double)",
            "System.Double System.Globalization.CalendricalCalculationsHelper::Angle(System.Int32,System.Int32,System.Double)",
            "System.Double System.Globalization.CalendricalCalculationsHelper::Angle(System.Int32,System.Int32,System.Double)",
            "System.Double System.Globalization.CalendricalCalculationsHelper::Angle(System.Int32,System.Int32,System.Double)",
            "System.Double System.Globalization.CalendricalCalculationsHelper::NormalizeLongitude(System.Double)",
            "System.Double System.Globalization.CalendricalCalculationsHelper::NormalizeLongitude(System.Double)",
            "System.Double System.Globalization.CalendricalCalculationsHelper::NormalizeLongitude(System.Double)",
            "System.Double System.Globalization.CalendricalCalculationsHelper::AsDayFraction(System.Double)",
            "System.Double System.Globalization.CalendricalCalculationsHelper::PolynomialSum(System.Double[],System.Double)",
            "System.Double System.Globalization.CalendricalCalculationsHelper::PolynomialSum(System.Double[],System.Double)",
            "System.Double System.Globalization.CalendricalCalculationsHelper::PolynomialSum(System.Double[],System.Double)",
            "System.Double System.Globalization.CalendricalCalculationsHelper::CenturiesFrom1900(System.Int32)",
            "System.Double System.Globalization.CalendricalCalculationsHelper::CenturiesFrom1900(System.Int32)",
            "System.Double System.Globalization.CalendricalCalculationsHelper::DefaultEphemerisCorrection(System.Int32)",
            "System.Double System.Globalization.CalendricalCalculationsHelper::DefaultEphemerisCorrection(System.Int32)",
            "System.Double System.Globalization.CalendricalCalculationsHelper::DefaultEphemerisCorrection(System.Int32)",
            "System.Double System.Globalization.CalendricalCalculationsHelper::DefaultEphemerisCorrection(System.Int32)",
            "System.Double System.Globalization.CalendricalCalculationsHelper::DefaultEphemerisCorrection(System.Int32)",
            "System.Double System.Globalization.CalendricalCalculationsHelper::DefaultEphemerisCorrection(System.Int32)",
            "System.Double System.Globalization.CalendricalCalculationsHelper::EphemerisCorrection1988to2019(System.Int32)",
            "System.Double System.Globalization.CalendricalCalculationsHelper::EphemerisCorrection1988to2019(System.Int32)",
            "System.Double System.Globalization.CalendricalCalculationsHelper::EphemerisCorrection1700to1799(System.Int32)",
            "System.Double System.Globalization.CalendricalCalculationsHelper::EphemerisCorrection1700to1799(System.Int32)",
            "System.Double System.Globalization.CalendricalCalculationsHelper::EphemerisCorrection1620to1699(System.Int32)",
            "System.Double System.Globalization.CalendricalCalculationsHelper::EphemerisCorrection1620to1699(System.Int32)",
            "System.Double System.Globalization.CalendricalCalculationsHelper::JulianCenturies(System.Double)",
            "System.Double System.Globalization.CalendricalCalculationsHelper::JulianCenturies(System.Double)",
            "System.Double System.Globalization.CalendricalCalculationsHelper::EquationOfTime(System.Double)",
            "System.Double System.Globalization.CalendricalCalculationsHelper::EquationOfTime(System.Double)",
            "System.Double System.Globalization.CalendricalCalculationsHelper::EquationOfTime(System.Double)",
            "System.Double System.Globalization.CalendricalCalculationsHelper::EquationOfTime(System.Double)",
            "System.Double System.Globalization.CalendricalCalculationsHelper::EquationOfTime(System.Double)",
            "System.Double System.Globalization.CalendricalCalculationsHelper::EquationOfTime(System.Double)",
            "System.Double System.Globalization.CalendricalCalculationsHelper::EquationOfTime(System.Double)",
            "System.Double System.Globalization.CalendricalCalculationsHelper::EquationOfTime(System.Double)",
            "System.Double System.Globalization.CalendricalCalculationsHelper::EquationOfTime(System.Double)",
            "System.Double System.Globalization.CalendricalCalculationsHelper::EquationOfTime(System.Double)",
            "System.Double System.Globalization.CalendricalCalculationsHelper::EquationOfTime(System.Double)",
            "System.Double System.Globalization.CalendricalCalculationsHelper::EquationOfTime(System.Double)",
            "System.Double System.Globalization.CalendricalCalculationsHelper::EquationOfTime(System.Double)",
            "System.Double System.Globalization.CalendricalCalculationsHelper::Midday(System.Double,System.Double)",
            "System.Double System.Globalization.CalendricalCalculationsHelper::InitLongitude(System.Double)",
            "System.Double System.Globalization.CalendricalCalculationsHelper::InitLongitude(System.Double)",
            "System.Double System.Globalization.CalendricalCalculationsHelper::MiddayAtPersianObservationSite(System.Double)",
            "System.Double System.Globalization.CalendricalCalculationsHelper::PeriodicTerm(System.Double,System.Int32,System.Double,System.Double)",
            "System.Double System.Globalization.CalendricalCalculationsHelper::SumLongSequenceOfPeriodicTerms(System.Double)",
            "System.Double System.Globalization.CalendricalCalculationsHelper::SumLongSequenceOfPeriodicTerms(System.Double)",
            "System.Double System.Globalization.CalendricalCalculationsHelper::SumLongSequenceOfPeriodicTerms(System.Double)",
            "System.Double System.Globalization.CalendricalCalculationsHelper::SumLongSequenceOfPeriodicTerms(System.Double)",
            "System.Double System.Globalization.CalendricalCalculationsHelper::SumLongSequenceOfPeriodicTerms(System.Double)",
            "System.Double System.Globalization.CalendricalCalculationsHelper::SumLongSequenceOfPeriodicTerms(System.Double)",
            "System.Double System.Globalization.CalendricalCalculationsHelper::SumLongSequenceOfPeriodicTerms(System.Double)",
            "System.Double System.Globalization.CalendricalCalculationsHelper::SumLongSequenceOfPeriodicTerms(System.Double)",
            "System.Double System.Globalization.CalendricalCalculationsHelper::SumLongSequenceOfPeriodicTerms(System.Double)",
            "System.Double System.Globalization.CalendricalCalculationsHelper::SumLongSequenceOfPeriodicTerms(System.Double)",
            "System.Double System.Globalization.CalendricalCalculationsHelper::SumLongSequenceOfPeriodicTerms(System.Double)",
            "System.Double System.Globalization.CalendricalCalculationsHelper::SumLongSequenceOfPeriodicTerms(System.Double)",
            "System.Double System.Globalization.CalendricalCalculationsHelper::SumLongSequenceOfPeriodicTerms(System.Double)",
            "System.Double System.Globalization.CalendricalCalculationsHelper::SumLongSequenceOfPeriodicTerms(System.Double)",
            "System.Double System.Globalization.CalendricalCalculationsHelper::SumLongSequenceOfPeriodicTerms(System.Double)",
            "System.Double System.Globalization.CalendricalCalculationsHelper::SumLongSequenceOfPeriodicTerms(System.Double)",
            "System.Double System.Globalization.CalendricalCalculationsHelper::SumLongSequenceOfPeriodicTerms(System.Double)",
            "System.Double System.Globalization.CalendricalCalculationsHelper::SumLongSequenceOfPeriodicTerms(System.Double)",
            "System.Double System.Globalization.CalendricalCalculationsHelper::SumLongSequenceOfPeriodicTerms(System.Double)",
            "System.Double System.Globalization.CalendricalCalculationsHelper::SumLongSequenceOfPeriodicTerms(System.Double)",
            "System.Double System.Globalization.CalendricalCalculationsHelper::SumLongSequenceOfPeriodicTerms(System.Double)",
            "System.Double System.Globalization.CalendricalCalculationsHelper::SumLongSequenceOfPeriodicTerms(System.Double)",
            "System.Double System.Globalization.CalendricalCalculationsHelper::SumLongSequenceOfPeriodicTerms(System.Double)",
            "System.Double System.Globalization.CalendricalCalculationsHelper::SumLongSequenceOfPeriodicTerms(System.Double)",
            "System.Double System.Globalization.CalendricalCalculationsHelper::SumLongSequenceOfPeriodicTerms(System.Double)",
            "System.Double System.Globalization.CalendricalCalculationsHelper::SumLongSequenceOfPeriodicTerms(System.Double)",
            "System.Double System.Globalization.CalendricalCalculationsHelper::SumLongSequenceOfPeriodicTerms(System.Double)",
            "System.Double System.Globalization.CalendricalCalculationsHelper::SumLongSequenceOfPeriodicTerms(System.Double)",
            "System.Double System.Globalization.CalendricalCalculationsHelper::SumLongSequenceOfPeriodicTerms(System.Double)",
            "System.Double System.Globalization.CalendricalCalculationsHelper::SumLongSequenceOfPeriodicTerms(System.Double)",
            "System.Double System.Globalization.CalendricalCalculationsHelper::SumLongSequenceOfPeriodicTerms(System.Double)",
            "System.Double System.Globalization.CalendricalCalculationsHelper::SumLongSequenceOfPeriodicTerms(System.Double)",
            "System.Double System.Globalization.CalendricalCalculationsHelper::SumLongSequenceOfPeriodicTerms(System.Double)",
            "System.Double System.Globalization.CalendricalCalculationsHelper::SumLongSequenceOfPeriodicTerms(System.Double)",
            "System.Double System.Globalization.CalendricalCalculationsHelper::SumLongSequenceOfPeriodicTerms(System.Double)",
            "System.Double System.Globalization.CalendricalCalculationsHelper::SumLongSequenceOfPeriodicTerms(System.Double)",
            "System.Double System.Globalization.CalendricalCalculationsHelper::SumLongSequenceOfPeriodicTerms(System.Double)",
            "System.Double System.Globalization.CalendricalCalculationsHelper::SumLongSequenceOfPeriodicTerms(System.Double)",
            "System.Double System.Globalization.CalendricalCalculationsHelper::SumLongSequenceOfPeriodicTerms(System.Double)",
            "System.Double System.Globalization.CalendricalCalculationsHelper::SumLongSequenceOfPeriodicTerms(System.Double)",
            "System.Double System.Globalization.CalendricalCalculationsHelper::SumLongSequenceOfPeriodicTerms(System.Double)",
            "System.Double System.Globalization.CalendricalCalculationsHelper::SumLongSequenceOfPeriodicTerms(System.Double)",
            "System.Double System.Globalization.CalendricalCalculationsHelper::SumLongSequenceOfPeriodicTerms(System.Double)",
            "System.Double System.Globalization.CalendricalCalculationsHelper::SumLongSequenceOfPeriodicTerms(System.Double)",
            "System.Double System.Globalization.CalendricalCalculationsHelper::SumLongSequenceOfPeriodicTerms(System.Double)",
            "System.Double System.Globalization.CalendricalCalculationsHelper::SumLongSequenceOfPeriodicTerms(System.Double)",
            "System.Double System.Globalization.CalendricalCalculationsHelper::SumLongSequenceOfPeriodicTerms(System.Double)",
            "System.Double System.Globalization.CalendricalCalculationsHelper::SumLongSequenceOfPeriodicTerms(System.Double)",
            "System.Double System.Globalization.CalendricalCalculationsHelper::SumLongSequenceOfPeriodicTerms(System.Double)",
            "System.Double System.Globalization.CalendricalCalculationsHelper::SumLongSequenceOfPeriodicTerms(System.Double)",
            "System.Double System.Globalization.CalendricalCalculationsHelper::SumLongSequenceOfPeriodicTerms(System.Double)",
            "System.Double System.Globalization.CalendricalCalculationsHelper::SumLongSequenceOfPeriodicTerms(System.Double)",
            "System.Double System.Globalization.CalendricalCalculationsHelper::SumLongSequenceOfPeriodicTerms(System.Double)",
            "System.Double System.Globalization.CalendricalCalculationsHelper::SumLongSequenceOfPeriodicTerms(System.Double)",
            "System.Double System.Globalization.CalendricalCalculationsHelper::SumLongSequenceOfPeriodicTerms(System.Double)",
            "System.Double System.Globalization.CalendricalCalculationsHelper::SumLongSequenceOfPeriodicTerms(System.Double)",
            "System.Double System.Globalization.CalendricalCalculationsHelper::SumLongSequenceOfPeriodicTerms(System.Double)",
            "System.Double System.Globalization.CalendricalCalculationsHelper::SumLongSequenceOfPeriodicTerms(System.Double)",
            "System.Double System.Globalization.CalendricalCalculationsHelper::SumLongSequenceOfPeriodicTerms(System.Double)",
            "System.Double System.Globalization.CalendricalCalculationsHelper::SumLongSequenceOfPeriodicTerms(System.Double)",
            "System.Double System.Globalization.CalendricalCalculationsHelper::SumLongSequenceOfPeriodicTerms(System.Double)",
            "System.Double System.Globalization.CalendricalCalculationsHelper::SumLongSequenceOfPeriodicTerms(System.Double)",
            "System.Double System.Globalization.CalendricalCalculationsHelper::SumLongSequenceOfPeriodicTerms(System.Double)",
            "System.Double System.Globalization.CalendricalCalculationsHelper::SumLongSequenceOfPeriodicTerms(System.Double)",
            "System.Double System.Globalization.CalendricalCalculationsHelper::SumLongSequenceOfPeriodicTerms(System.Double)",
            "System.Double System.Globalization.CalendricalCalculationsHelper::SumLongSequenceOfPeriodicTerms(System.Double)",
            "System.Double System.Globalization.CalendricalCalculationsHelper::SumLongSequenceOfPeriodicTerms(System.Double)",
            "System.Double System.Globalization.CalendricalCalculationsHelper::SumLongSequenceOfPeriodicTerms(System.Double)",
            "System.Double System.Globalization.CalendricalCalculationsHelper::SumLongSequenceOfPeriodicTerms(System.Double)",
            "System.Double System.Globalization.CalendricalCalculationsHelper::SumLongSequenceOfPeriodicTerms(System.Double)",
            "System.Double System.Globalization.CalendricalCalculationsHelper::SumLongSequenceOfPeriodicTerms(System.Double)",
            "System.Double System.Globalization.CalendricalCalculationsHelper::SumLongSequenceOfPeriodicTerms(System.Double)",
            "System.Double System.Globalization.CalendricalCalculationsHelper::SumLongSequenceOfPeriodicTerms(System.Double)",
            "System.Double System.Globalization.CalendricalCalculationsHelper::SumLongSequenceOfPeriodicTerms(System.Double)",
            "System.Double System.Globalization.CalendricalCalculationsHelper::SumLongSequenceOfPeriodicTerms(System.Double)",
            "System.Double System.Globalization.CalendricalCalculationsHelper::SumLongSequenceOfPeriodicTerms(System.Double)",
            "System.Double System.Globalization.CalendricalCalculationsHelper::SumLongSequenceOfPeriodicTerms(System.Double)",
            "System.Double System.Globalization.CalendricalCalculationsHelper::SumLongSequenceOfPeriodicTerms(System.Double)",
            "System.Double System.Globalization.CalendricalCalculationsHelper::SumLongSequenceOfPeriodicTerms(System.Double)",
            "System.Double System.Globalization.CalendricalCalculationsHelper::SumLongSequenceOfPeriodicTerms(System.Double)",
            "System.Double System.Globalization.CalendricalCalculationsHelper::SumLongSequenceOfPeriodicTerms(System.Double)",
            "System.Double System.Globalization.CalendricalCalculationsHelper::SumLongSequenceOfPeriodicTerms(System.Double)",
            "System.Double System.Globalization.CalendricalCalculationsHelper::SumLongSequenceOfPeriodicTerms(System.Double)",
            "System.Double System.Globalization.CalendricalCalculationsHelper::SumLongSequenceOfPeriodicTerms(System.Double)",
            "System.Double System.Globalization.CalendricalCalculationsHelper::SumLongSequenceOfPeriodicTerms(System.Double)",
            "System.Double System.Globalization.CalendricalCalculationsHelper::SumLongSequenceOfPeriodicTerms(System.Double)",
            "System.Double System.Globalization.CalendricalCalculationsHelper::SumLongSequenceOfPeriodicTerms(System.Double)",
            "System.Double System.Globalization.CalendricalCalculationsHelper::SumLongSequenceOfPeriodicTerms(System.Double)",
            "System.Double System.Globalization.CalendricalCalculationsHelper::SumLongSequenceOfPeriodicTerms(System.Double)",
            "System.Double System.Globalization.CalendricalCalculationsHelper::SumLongSequenceOfPeriodicTerms(System.Double)",
            "System.Double System.Globalization.CalendricalCalculationsHelper::SumLongSequenceOfPeriodicTerms(System.Double)",
            "System.Double System.Globalization.CalendricalCalculationsHelper::SumLongSequenceOfPeriodicTerms(System.Double)",
            "System.Double System.Globalization.CalendricalCalculationsHelper::SumLongSequenceOfPeriodicTerms(System.Double)",
            "System.Double System.Globalization.CalendricalCalculationsHelper::SumLongSequenceOfPeriodicTerms(System.Double)",
            "System.Double System.Globalization.CalendricalCalculationsHelper::SumLongSequenceOfPeriodicTerms(System.Double)",
            "System.Double System.Globalization.CalendricalCalculationsHelper::SumLongSequenceOfPeriodicTerms(System.Double)",
            "System.Double System.Globalization.CalendricalCalculationsHelper::SumLongSequenceOfPeriodicTerms(System.Double)",
            "System.Double System.Globalization.CalendricalCalculationsHelper::SumLongSequenceOfPeriodicTerms(System.Double)",
            "System.Double System.Globalization.CalendricalCalculationsHelper::SumLongSequenceOfPeriodicTerms(System.Double)",
            "System.Double System.Globalization.CalendricalCalculationsHelper::Aberration(System.Double)",
            "System.Double System.Globalization.CalendricalCalculationsHelper::Aberration(System.Double)",
            "System.Double System.Globalization.CalendricalCalculationsHelper::Aberration(System.Double)",
            "System.Double System.Globalization.CalendricalCalculationsHelper::Aberration(System.Double)",
            "System.Double System.Globalization.CalendricalCalculationsHelper::Nutation(System.Double)",
            "System.Double System.Globalization.CalendricalCalculationsHelper::Nutation(System.Double)",
            "System.Double System.Globalization.CalendricalCalculationsHelper::Compute(System.Double)",
            "System.Double System.Globalization.CalendricalCalculationsHelper::Compute(System.Double)",
            "System.Double System.Globalization.CalendricalCalculationsHelper::Compute(System.Double)",
            "System.Double System.Globalization.CalendricalCalculationsHelper::AsSeason(System.Double)",
            "System.Double System.Globalization.CalendricalCalculationsHelper::AsSeason(System.Double)",
            "System.Double System.Globalization.CalendricalCalculationsHelper::EstimatePrior(System.Double,System.Double)",
            "System.Double System.Globalization.CalendricalCalculationsHelper::EstimatePrior(System.Double,System.Double)",
            "System.Double System.StubHelpers.DateMarshaler::ConvertToNative(System.DateTime)",
            "System.Reflection.AssemblyName System.Reflection.AssemblyName::nGetFileInformation(System.String)",
            "System.Reflection.RuntimeAssembly System.Reflection.RuntimeAssembly::_nLoad(System.Reflection.AssemblyName,System.String,System.Security.Policy.Evidence,System.Reflection.RuntimeAssembly,System.Threading.StackCrawlMark&,System.IntPtr,System.Boolean,System.Boolean,System.Boolean,System.IntPtr)",
            "System.Reflection.AssemblyName[] System.Reflection.RuntimeAssembly::GetReferencedAssemblies(System.Reflection.RuntimeAssembly)",
            "System.Reflection.AssemblyNameFlags System.Reflection.RuntimeAssembly::GetFlags(System.Reflection.RuntimeAssembly)",
            "System.Reflection.RuntimeModule System.Reflection.RuntimeAssembly::GetManifestModule(System.Reflection.RuntimeAssembly)",
            "System.RuntimeType[] System.Reflection.RuntimeModule::GetTypes(System.Reflection.RuntimeModule)",
            "System.Reflection.RuntimeModule System.Reflection.Emit.AssemblyBuilder::GetInMemoryAssemblyModule(System.Reflection.RuntimeAssembly)",
            "System.Reflection.Assembly System.Reflection.Emit.AssemblyBuilder::nCreateDynamicAssembly(System.AppDomain,System.Reflection.AssemblyName,System.Security.Policy.Evidence,System.Threading.StackCrawlMark&,System.Byte[],System.Byte[],System.Reflection.Emit.AssemblyBuilderAccess,System.Reflection.Emit.DynamicAssemblyFlags,System.Security.SecurityContextSource)",
            "System.Runtime.InteropServices.WindowsRuntime.IRestrictedErrorInfo System.Runtime.InteropServices.WindowsRuntime.UnsafeNativeMethods::GetRestrictedErrorInfo()"
        };

        public IEnumerable<ValidationResult> Validate(MethodDefinition method)
        {
            foreach (Mono.Cecil.Cil.Instruction instruction in method.Body.Instructions)
            {
                if (instruction.Operand is MethodReference methodReference)
                {
                    if (RedLightMethods.Contains(methodReference.FullName))
                    {
                        return new []
                        {
                            new MethodDefinitionValidationResult(
                                method.Name,
                                ErrorType,
                                $"Use of {method.FullName} is non-deterministic [{ErrorType} ({methodReference.FullName})]")
                        };
                    }
                }
            }

            return Enumerable.Empty<MethodDefinitionValidationResult>();
        }
    }
}