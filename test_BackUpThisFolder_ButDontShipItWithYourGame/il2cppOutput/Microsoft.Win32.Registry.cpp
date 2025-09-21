#include "pch-cpp.hpp"

#ifndef _MSC_VER
# include <alloca.h>
#else
# include <malloc.h>
#endif


#include <limits>


template <typename R>
struct VirtualFuncInvoker0
{
	typedef R (*Func)(void*, const RuntimeMethod*);

	static inline R Invoke (Il2CppMethodSlot slot, RuntimeObject* obj)
	{
		const VirtualInvokeData& invokeData = il2cpp_codegen_get_virtual_invoke_data(slot, obj);
		return ((Func)invokeData.methodPtr)(obj, invokeData.method);
	}
};
template <typename R, typename T1>
struct VirtualFuncInvoker1
{
	typedef R (*Func)(void*, T1, const RuntimeMethod*);

	static inline R Invoke (Il2CppMethodSlot slot, RuntimeObject* obj, T1 p1)
	{
		const VirtualInvokeData& invokeData = il2cpp_codegen_get_virtual_invoke_data(slot, obj);
		return ((Func)invokeData.methodPtr)(obj, p1, invokeData.method);
	}
};

struct Dictionary_2_tAC32B254416DD510DC3E7E36B0706A6B031D7A53;
struct ByteU5BU5D_tA6237BF417AE52AD70CFB4EF24A7A82613DF9031;
struct CharU5BU5D_t799905CF001DD5F13F7DBB310181FC4D8B7D0AAB;
struct IntPtrU5BU5D_tFD177F8C806A6921AD7150264CCC62FA00CAD832;
struct ObjectU5BU5D_t8061030B0A12A55D5AD8652A20C922FE99450918;
struct StackTraceU5BU5D_t32FBCB20930EAF5BAE3F450FF75228E5450DA0DF;
struct StringU5BU5D_t7674CD946EC0CE7B3AE0BE70E6EE85F2ECD9F248;
struct TypeU5BU5D_t97234E1129B564EB38B8D85CAC2AD8B5B9522FFB;
struct ArgumentException_tAD90411542A20A9C72D5CDA3A84181D8B947A263;
struct ArgumentNullException_t327031E412FAB2351B0022DD5DAD47E67E597129;
struct Assembly_t;
struct Attribute_tFDA8EFEFB0711976D22474794576DAF28F7440AA;
struct Binder_t91BFCE95A7057FADF4D8A1A342AFE52872246235;
struct CultureInfo_t9BA817D41AD55AC8BD07480DD8AC22F8FFA378E0;
struct EmbeddedAttribute_tEEA5A57B9AF7201C983759F50BEE83FFC2EC27EC;
struct Hashtable_tEFC3B6496E6747787D8BB761B51F2AE3A8CFFE2D;
struct IDictionary_t6D03155AF1FA9083817AA5B6AD7DEEACC26AB220;
struct IResourceGroveler_tDEE701BD41E9E5D260606F79F75427B42C4CC0C0;
struct MemberFilter_tF644F1AE82F611B677CE1964D5A3277DDA21D553;
struct NullableAttribute_tBE95F229CD35EA5CFBC09238521069226F6805AF;
struct NullableContextAttribute_t243FAEC75CCC2C5D4BF17411FFE0C81F640A2606;
struct NullablePublicOnlyAttribute_t3BF10572C5F45E4F2CD63F7938BAE17FD740114C;
struct OSPlatformAttribute_t7542A2BF18E4C64EED099C1FB8150B228C7EF68B;
struct ObjectDisposedException_tC5FB29E8E980E2010A2F6A5B9B791089419F89EB;
struct PlatformNotSupportedException_tD2BD7EB9278518AA5FE8AE75AD5D0D4298A4631A;
struct RegistryKey_t8752E3D024B69A6A9CC30AEB0E75F590C74E30D4;
struct ResourceManager_t311D6D32A753224008949B32CC6A5468C47498EB;
struct RuntimeAssembly_tA26A4DE82E77826DFC3D58AD976BCFC6BCA918AF;
struct SafeHandle_tC1A4DA80DA89B867CC011B707A07275230321BF7;
struct SafeRegistryHandle_t890BD43C81043709A2103F1FDC4394C603EE1FDF;
struct SafeSerializationManager_tCBB85B95DFD1634237140CD892E82D06ECB3F5E6;
struct String_t;
struct StringBuilder_t;
struct SupportedOSPlatformAttribute_t5154EE63A81CFEFDCFDAC80983E85D33825F7C2C;
struct Type_t;
struct Version_tE426DB5655D0F22920AE16A2AA9AB7781B8255A7;
struct Void_t4861ACF8F4594C3437BB48B6E56783494B843915;
struct CultureNameResourceSetPair_t06C5772C09CA853E70E42C0E8BC57FE0AA2CB674;

IL2CPP_EXTERN_C RuntimeClass* AppContext_t0380D19FAC72CD59D46947D86DC1DAA3BCE638E0_il2cpp_TypeInfo_var;
IL2CPP_EXTERN_C RuntimeClass* ArgumentException_tAD90411542A20A9C72D5CDA3A84181D8B947A263_il2cpp_TypeInfo_var;
IL2CPP_EXTERN_C RuntimeClass* ArgumentNullException_t327031E412FAB2351B0022DD5DAD47E67E597129_il2cpp_TypeInfo_var;
IL2CPP_EXTERN_C RuntimeClass* ByteU5BU5D_tA6237BF417AE52AD70CFB4EF24A7A82613DF9031_il2cpp_TypeInfo_var;
IL2CPP_EXTERN_C RuntimeClass* IOException_t5D599190B003D41D45D4839A9B6B9AB53A755910_il2cpp_TypeInfo_var;
IL2CPP_EXTERN_C RuntimeClass* MissingManifestResourceException_t136A089345909ADB6333D6F4E2AA84C7A00CB3FD_il2cpp_TypeInfo_var;
IL2CPP_EXTERN_C RuntimeClass* ObjectDisposedException_tC5FB29E8E980E2010A2F6A5B9B791089419F89EB_il2cpp_TypeInfo_var;
IL2CPP_EXTERN_C RuntimeClass* PlatformNotSupportedException_tD2BD7EB9278518AA5FE8AE75AD5D0D4298A4631A_il2cpp_TypeInfo_var;
IL2CPP_EXTERN_C RuntimeClass* RegistryKey_t8752E3D024B69A6A9CC30AEB0E75F590C74E30D4_il2cpp_TypeInfo_var;
IL2CPP_EXTERN_C RuntimeClass* Registry_tE2BC25EC64E312939C45900E01D9B533CBE70024_il2cpp_TypeInfo_var;
IL2CPP_EXTERN_C RuntimeClass* ResourceManager_t311D6D32A753224008949B32CC6A5468C47498EB_il2cpp_TypeInfo_var;
IL2CPP_EXTERN_C RuntimeClass* SR_tCF5D02DC363D3707E4B0700773B397B107D749CF_il2cpp_TypeInfo_var;
IL2CPP_EXTERN_C RuntimeClass* StringBuilder_t_il2cpp_TypeInfo_var;
IL2CPP_EXTERN_C RuntimeClass* StringU5BU5D_t7674CD946EC0CE7B3AE0BE70E6EE85F2ECD9F248_il2cpp_TypeInfo_var;
IL2CPP_EXTERN_C RuntimeClass* Type_t_il2cpp_TypeInfo_var;
IL2CPP_EXTERN_C String_t* _stringLiteral09B11B6CC411D8B9FFB75EAAE9A35B2AF248CE40;
IL2CPP_EXTERN_C String_t* _stringLiteral1D987D14D8E0C888F1095B9A3F3E261A95CEACCC;
IL2CPP_EXTERN_C String_t* _stringLiteral21E1B4354893A602D3EAE4BBA57311B083D8E0A0;
IL2CPP_EXTERN_C String_t* _stringLiteral3792CAE3D944C750A90C6EAB820EBB80F23128A8;
IL2CPP_EXTERN_C String_t* _stringLiteral592A968CA63418B09EC2CE77B5A94E7EC7109F1F;
IL2CPP_EXTERN_C String_t* _stringLiteral6BBACEC2AF7F52E71DDFBD94D23CB7140B770916;
IL2CPP_EXTERN_C String_t* _stringLiteral86A1D5983E899BFCC1B8D83C44231A4F60497E4D;
IL2CPP_EXTERN_C String_t* _stringLiteral8A047FD9B4CCBDFD3876EB4B4AB623EF03671DC3;
IL2CPP_EXTERN_C String_t* _stringLiteralA0324410B1B7CC964150BD7DC521C17C3491F407;
IL2CPP_EXTERN_C String_t* _stringLiteralB5528CA07A43AEA4EFA2F7B2DEF38E0A5D87ECD6;
IL2CPP_EXTERN_C String_t* _stringLiteralCD83C95652A081F6D88BF9BB6415F31CED9C449E;
IL2CPP_EXTERN_C String_t* _stringLiteralCE18B047107AA23D1AA9B2ED32D316148E02655F;
IL2CPP_EXTERN_C String_t* _stringLiteralDC92678F646C9D9E1B7EB843CE840E2B0420D5BF;
IL2CPP_EXTERN_C String_t* _stringLiteralFD48E940AB4046C2C8344BD46CB54A2ACDC31BD4;
IL2CPP_EXTERN_C const RuntimeMethod* Array_Empty_TisString_t_m9832B70DF2B936246FE60F75D3D12CB946C39D16_RuntimeMethod_var;
IL2CPP_EXTERN_C const RuntimeMethod* RegistryKey_ClosePerfDataKey_mA9A7893C2D9C587AD7F750CB3AF290BDAF57EE39_RuntimeMethod_var;
IL2CPP_EXTERN_C const RuntimeMethod* RegistryKey_EnsureNotDisposed_m8BE19DC7F1E7B9C2123D3BB6416905EFE68AFD78_RuntimeMethod_var;
IL2CPP_EXTERN_C const RuntimeMethod* RegistryKey_InternalGetSubKeyNamesCore_m715FD9D02D73A1BE3FA48509AB2378A1880A798A_RuntimeMethod_var;
IL2CPP_EXTERN_C const RuntimeMethod* RegistryKey_InternalGetValueCore_m139A3171C748AAD22E972CB0048FD87655B00ACA_RuntimeMethod_var;
IL2CPP_EXTERN_C const RuntimeMethod* RegistryKey_InternalOpenSubKeyCore_mD0430381A6C05276BF333D5FF370C3CED944BFB3_RuntimeMethod_var;
IL2CPP_EXTERN_C const RuntimeMethod* RegistryKey_InternalSubKeyCountCore_mC707C8439FDE18743C1ADB336C72E30C83B61995_RuntimeMethod_var;
IL2CPP_EXTERN_C const RuntimeMethod* RegistryKey_OpenBaseKeyCore_mC9589E517BD09EBA2BAAFE07DEFA4D5D7D992151_RuntimeMethod_var;
IL2CPP_EXTERN_C const RuntimeMethod* RegistryKey_ValidateKeyName_m738969C019B38EEA5B8C3C25BB7D4576FE0EE203_RuntimeMethod_var;
IL2CPP_EXTERN_C const RuntimeMethod* RegistryKey_ValidateKeyView_m6750A1F58ACF19E810049D095CCF0744807879F0_RuntimeMethod_var;
IL2CPP_EXTERN_C const RuntimeType* SR_t17E2A436BA997C55733E1B4C195401F61766CEF9_0_0_0_var;
struct Exception_t_marshaled_com;
struct Exception_t_marshaled_pinvoke;

struct ByteU5BU5D_tA6237BF417AE52AD70CFB4EF24A7A82613DF9031;
struct ObjectU5BU5D_t8061030B0A12A55D5AD8652A20C922FE99450918;
struct StringU5BU5D_t7674CD946EC0CE7B3AE0BE70E6EE85F2ECD9F248;

IL2CPP_EXTERN_C_BEGIN
IL2CPP_EXTERN_C_END

#ifdef __clang__
#pragma clang diagnostic push
#pragma clang diagnostic ignored "-Winvalid-offsetof"
#pragma clang diagnostic ignored "-Wunused-variable"
#endif
struct U3CModuleU3E_tE1998C42828AB39FE217AE5CD7107717C544143D 
{
};
struct EmptyArray_1_tDF0DD7256B115243AA6BD5558417387A734240EE  : public RuntimeObject
{
};
struct Attribute_tFDA8EFEFB0711976D22474794576DAF28F7440AA  : public RuntimeObject
{
};
struct CriticalFinalizerObject_t1DCAB623CAEA6529A96F5F3EDE3C7048A6E313C9  : public RuntimeObject
{
};
struct MarshalByRefObject_t8C2F4C5854177FD60439EB1FCCFC1B3CFAFE8DCE  : public RuntimeObject
{
	RuntimeObject* ____identity;
};
struct MarshalByRefObject_t8C2F4C5854177FD60439EB1FCCFC1B3CFAFE8DCE_marshaled_pinvoke
{
	Il2CppIUnknown* ____identity;
};
struct MarshalByRefObject_t8C2F4C5854177FD60439EB1FCCFC1B3CFAFE8DCE_marshaled_com
{
	Il2CppIUnknown* ____identity;
};
struct MemberInfo_t  : public RuntimeObject
{
};
struct Registry_tE2BC25EC64E312939C45900E01D9B533CBE70024  : public RuntimeObject
{
};
struct ResourceManager_t311D6D32A753224008949B32CC6A5468C47498EB  : public RuntimeObject
{
	String_t* ___BaseNameField;
	Hashtable_tEFC3B6496E6747787D8BB761B51F2AE3A8CFFE2D* ___ResourceSets;
	Dictionary_2_tAC32B254416DD510DC3E7E36B0706A6B031D7A53* ____resourceSets;
	String_t* ___moduleDir;
	Assembly_t* ___MainAssembly;
	Type_t* ____locationInfo;
	Type_t* ____userResourceSet;
	CultureInfo_t9BA817D41AD55AC8BD07480DD8AC22F8FFA378E0* ____neutralResourcesCulture;
	CultureNameResourceSetPair_t06C5772C09CA853E70E42C0E8BC57FE0AA2CB674* ____lastUsedResourceCache;
	bool ____ignoreCase;
	bool ___UseManifest;
	bool ___UseSatelliteAssem;
	int32_t ____fallbackLoc;
	Version_tE426DB5655D0F22920AE16A2AA9AB7781B8255A7* ____satelliteContractVersion;
	bool ____lookedForSatelliteContractVersion;
	Assembly_t* ____callingAssembly;
	RuntimeAssembly_tA26A4DE82E77826DFC3D58AD976BCFC6BCA918AF* ___m_callingAssembly;
	RuntimeObject* ___resourceGroveler;
};
struct SR_t17E2A436BA997C55733E1B4C195401F61766CEF9  : public RuntimeObject
{
};
struct SR_tCF5D02DC363D3707E4B0700773B397B107D749CF  : public RuntimeObject
{
};
struct String_t  : public RuntimeObject
{
	int32_t ____stringLength;
	Il2CppChar ____firstChar;
};
struct StringBuilder_t  : public RuntimeObject
{
	CharU5BU5D_t799905CF001DD5F13F7DBB310181FC4D8B7D0AAB* ___m_ChunkChars;
	StringBuilder_t* ___m_ChunkPrevious;
	int32_t ___m_ChunkLength;
	int32_t ___m_ChunkOffset;
	int32_t ___m_MaxCapacity;
};
struct ValueType_t6D9B272BD21782F0A9A14F2E41F85A50E97A986F  : public RuntimeObject
{
};
struct ValueType_t6D9B272BD21782F0A9A14F2E41F85A50E97A986F_marshaled_pinvoke
{
};
struct ValueType_t6D9B272BD21782F0A9A14F2E41F85A50E97A986F_marshaled_com
{
};
struct Boolean_t09A6377A54BE2F9E6985A8149F19234FD7DDFE22 
{
	bool ___m_value;
};
struct Byte_t94D9231AC217BE4D2E004C4CD32DF6D099EA41A3 
{
	uint8_t ___m_value;
};
struct Char_t521A6F19B456D956AF452D926C32709DC03D6B17 
{
	Il2CppChar ___m_value;
};
struct EmbeddedAttribute_tEEA5A57B9AF7201C983759F50BEE83FFC2EC27EC  : public Attribute_tFDA8EFEFB0711976D22474794576DAF28F7440AA
{
};
struct Int32_t680FF22E76F6EFAD4375103CBBFFA0421349384C 
{
	int32_t ___m_value;
};
struct IntPtr_t 
{
	void* ___m_value;
};
struct NullableAttribute_tBE95F229CD35EA5CFBC09238521069226F6805AF  : public Attribute_tFDA8EFEFB0711976D22474794576DAF28F7440AA
{
	ByteU5BU5D_tA6237BF417AE52AD70CFB4EF24A7A82613DF9031* ___NullableFlags;
};
struct NullableContextAttribute_t243FAEC75CCC2C5D4BF17411FFE0C81F640A2606  : public Attribute_tFDA8EFEFB0711976D22474794576DAF28F7440AA
{
	uint8_t ___Flag;
};
struct NullablePublicOnlyAttribute_t3BF10572C5F45E4F2CD63F7938BAE17FD740114C  : public Attribute_tFDA8EFEFB0711976D22474794576DAF28F7440AA
{
	bool ___IncludesInternals;
};
struct OSPlatformAttribute_t7542A2BF18E4C64EED099C1FB8150B228C7EF68B  : public Attribute_tFDA8EFEFB0711976D22474794576DAF28F7440AA
{
	String_t* ___U3CPlatformNameU3Ek__BackingField;
};
struct RegistryKey_t8752E3D024B69A6A9CC30AEB0E75F590C74E30D4  : public MarshalByRefObject_t8C2F4C5854177FD60439EB1FCCFC1B3CFAFE8DCE
{
	SafeRegistryHandle_t890BD43C81043709A2103F1FDC4394C603EE1FDF* ____hkey;
	String_t* ____keyName;
	int32_t ____state;
};
struct Void_t4861ACF8F4594C3437BB48B6E56783494B843915 
{
	union
	{
		struct
		{
		};
		uint8_t Void_t4861ACF8F4594C3437BB48B6E56783494B843915__padding[1];
	};
};
struct Exception_t  : public RuntimeObject
{
	String_t* ____className;
	String_t* ____message;
	RuntimeObject* ____data;
	Exception_t* ____innerException;
	String_t* ____helpURL;
	RuntimeObject* ____stackTrace;
	String_t* ____stackTraceString;
	String_t* ____remoteStackTraceString;
	int32_t ____remoteStackIndex;
	RuntimeObject* ____dynamicMethods;
	int32_t ____HResult;
	String_t* ____source;
	SafeSerializationManager_tCBB85B95DFD1634237140CD892E82D06ECB3F5E6* ____safeSerializationManager;
	StackTraceU5BU5D_t32FBCB20930EAF5BAE3F450FF75228E5450DA0DF* ___captured_traces;
	IntPtrU5BU5D_tFD177F8C806A6921AD7150264CCC62FA00CAD832* ___native_trace_ips;
	int32_t ___caught_in_unmanaged;
};
struct Exception_t_marshaled_pinvoke
{
	char* ____className;
	char* ____message;
	RuntimeObject* ____data;
	Exception_t_marshaled_pinvoke* ____innerException;
	char* ____helpURL;
	Il2CppIUnknown* ____stackTrace;
	char* ____stackTraceString;
	char* ____remoteStackTraceString;
	int32_t ____remoteStackIndex;
	Il2CppIUnknown* ____dynamicMethods;
	int32_t ____HResult;
	char* ____source;
	SafeSerializationManager_tCBB85B95DFD1634237140CD892E82D06ECB3F5E6* ____safeSerializationManager;
	StackTraceU5BU5D_t32FBCB20930EAF5BAE3F450FF75228E5450DA0DF* ___captured_traces;
	Il2CppSafeArray* ___native_trace_ips;
	int32_t ___caught_in_unmanaged;
};
struct Exception_t_marshaled_com
{
	Il2CppChar* ____className;
	Il2CppChar* ____message;
	RuntimeObject* ____data;
	Exception_t_marshaled_com* ____innerException;
	Il2CppChar* ____helpURL;
	Il2CppIUnknown* ____stackTrace;
	Il2CppChar* ____stackTraceString;
	Il2CppChar* ____remoteStackTraceString;
	int32_t ____remoteStackIndex;
	Il2CppIUnknown* ____dynamicMethods;
	int32_t ____HResult;
	Il2CppChar* ____source;
	SafeSerializationManager_tCBB85B95DFD1634237140CD892E82D06ECB3F5E6* ____safeSerializationManager;
	StackTraceU5BU5D_t32FBCB20930EAF5BAE3F450FF75228E5450DA0DF* ___captured_traces;
	Il2CppSafeArray* ___native_trace_ips;
	int32_t ___caught_in_unmanaged;
};
struct RuntimeTypeHandle_t332A452B8B6179E4469B69525D0FE82A88030F7B 
{
	intptr_t ___value;
};
struct SafeHandle_tC1A4DA80DA89B867CC011B707A07275230321BF7  : public CriticalFinalizerObject_t1DCAB623CAEA6529A96F5F3EDE3C7048A6E313C9
{
	intptr_t ___handle;
	int32_t ____state;
	bool ____ownsHandle;
	bool ____fullyInitialized;
};
struct SupportedOSPlatformAttribute_t5154EE63A81CFEFDCFDAC80983E85D33825F7C2C  : public OSPlatformAttribute_t7542A2BF18E4C64EED099C1FB8150B228C7EF68B
{
};
struct SafeHandleZeroOrMinusOneIsInvalid_tC152552D137451170B3B1A304227B0ECADB65629  : public SafeHandle_tC1A4DA80DA89B867CC011B707A07275230321BF7
{
};
struct SystemException_tCC48D868298F4C0705279823E34B00F4FBDB7295  : public Exception_t
{
};
struct Type_t  : public MemberInfo_t
{
	RuntimeTypeHandle_t332A452B8B6179E4469B69525D0FE82A88030F7B ____impl;
};
struct ArgumentException_tAD90411542A20A9C72D5CDA3A84181D8B947A263  : public SystemException_tCC48D868298F4C0705279823E34B00F4FBDB7295
{
	String_t* ____paramName;
};
struct IOException_t5D599190B003D41D45D4839A9B6B9AB53A755910  : public SystemException_tCC48D868298F4C0705279823E34B00F4FBDB7295
{
};
struct InvalidOperationException_t5DDE4D49B7405FAAB1E4576F4715A42A3FAD4BAB  : public SystemException_tCC48D868298F4C0705279823E34B00F4FBDB7295
{
};
struct MissingManifestResourceException_t136A089345909ADB6333D6F4E2AA84C7A00CB3FD  : public SystemException_tCC48D868298F4C0705279823E34B00F4FBDB7295
{
};
struct NotSupportedException_t1429765983D409BD2986508963C98D214E4EBF4A  : public SystemException_tCC48D868298F4C0705279823E34B00F4FBDB7295
{
};
struct SafeRegistryHandle_t890BD43C81043709A2103F1FDC4394C603EE1FDF  : public SafeHandleZeroOrMinusOneIsInvalid_tC152552D137451170B3B1A304227B0ECADB65629
{
};
struct ArgumentNullException_t327031E412FAB2351B0022DD5DAD47E67E597129  : public ArgumentException_tAD90411542A20A9C72D5CDA3A84181D8B947A263
{
};
struct ObjectDisposedException_tC5FB29E8E980E2010A2F6A5B9B791089419F89EB  : public InvalidOperationException_t5DDE4D49B7405FAAB1E4576F4715A42A3FAD4BAB
{
	String_t* ____objectName;
};
struct PlatformNotSupportedException_tD2BD7EB9278518AA5FE8AE75AD5D0D4298A4631A  : public NotSupportedException_t1429765983D409BD2986508963C98D214E4EBF4A
{
};
struct EmptyArray_1_tDF0DD7256B115243AA6BD5558417387A734240EE_StaticFields
{
	ObjectU5BU5D_t8061030B0A12A55D5AD8652A20C922FE99450918* ___Value;
};
struct Registry_tE2BC25EC64E312939C45900E01D9B533CBE70024_StaticFields
{
	RegistryKey_t8752E3D024B69A6A9CC30AEB0E75F590C74E30D4* ___CurrentUser;
	RegistryKey_t8752E3D024B69A6A9CC30AEB0E75F590C74E30D4* ___LocalMachine;
	RegistryKey_t8752E3D024B69A6A9CC30AEB0E75F590C74E30D4* ___ClassesRoot;
	RegistryKey_t8752E3D024B69A6A9CC30AEB0E75F590C74E30D4* ___Users;
	RegistryKey_t8752E3D024B69A6A9CC30AEB0E75F590C74E30D4* ___PerformanceData;
	RegistryKey_t8752E3D024B69A6A9CC30AEB0E75F590C74E30D4* ___CurrentConfig;
};
struct ResourceManager_t311D6D32A753224008949B32CC6A5468C47498EB_StaticFields
{
	int32_t ___MagicNumber;
	int32_t ___HeaderVersionNumber;
	Type_t* ____minResourceSet;
	String_t* ___ResReaderTypeName;
	String_t* ___ResSetTypeName;
	String_t* ___MscorlibName;
	int32_t ___DEBUG;
};
struct SR_tCF5D02DC363D3707E4B0700773B397B107D749CF_StaticFields
{
	bool ___s_usingResourceKeys;
	ResourceManager_t311D6D32A753224008949B32CC6A5468C47498EB* ___s_resourceManager;
};
struct String_t_StaticFields
{
	String_t* ___Empty;
};
struct Boolean_t09A6377A54BE2F9E6985A8149F19234FD7DDFE22_StaticFields
{
	String_t* ___TrueString;
	String_t* ___FalseString;
};
struct Char_t521A6F19B456D956AF452D926C32709DC03D6B17_StaticFields
{
	ByteU5BU5D_tA6237BF417AE52AD70CFB4EF24A7A82613DF9031* ___s_categoryForLatin1;
};
struct IntPtr_t_StaticFields
{
	intptr_t ___Zero;
};
struct RegistryKey_t8752E3D024B69A6A9CC30AEB0E75F590C74E30D4_StaticFields
{
	intptr_t ___HKEY_CLASSES_ROOT;
	intptr_t ___HKEY_CURRENT_USER;
	intptr_t ___HKEY_LOCAL_MACHINE;
	intptr_t ___HKEY_USERS;
	intptr_t ___HKEY_PERFORMANCE_DATA;
	intptr_t ___HKEY_CURRENT_CONFIG;
	StringU5BU5D_t7674CD946EC0CE7B3AE0BE70E6EE85F2ECD9F248* ___s_hkeyNames;
};
struct Type_t_StaticFields
{
	Binder_t91BFCE95A7057FADF4D8A1A342AFE52872246235* ___s_defaultBinder;
	Il2CppChar ___Delimiter;
	TypeU5BU5D_t97234E1129B564EB38B8D85CAC2AD8B5B9522FFB* ___EmptyTypes;
	RuntimeObject* ___Missing;
	MemberFilter_tF644F1AE82F611B677CE1964D5A3277DDA21D553* ___FilterAttribute;
	MemberFilter_tF644F1AE82F611B677CE1964D5A3277DDA21D553* ___FilterName;
	MemberFilter_tF644F1AE82F611B677CE1964D5A3277DDA21D553* ___FilterNameIgnoreCase;
};
#ifdef __clang__
#pragma clang diagnostic pop
#endif
struct ByteU5BU5D_tA6237BF417AE52AD70CFB4EF24A7A82613DF9031  : public RuntimeArray
{
	ALIGN_FIELD (8) uint8_t m_Items[1];

	inline uint8_t GetAt(il2cpp_array_size_t index) const
	{
		IL2CPP_ARRAY_BOUNDS_CHECK(index, (uint32_t)(this)->max_length);
		return m_Items[index];
	}
	inline uint8_t* GetAddressAt(il2cpp_array_size_t index)
	{
		IL2CPP_ARRAY_BOUNDS_CHECK(index, (uint32_t)(this)->max_length);
		return m_Items + index;
	}
	inline void SetAt(il2cpp_array_size_t index, uint8_t value)
	{
		IL2CPP_ARRAY_BOUNDS_CHECK(index, (uint32_t)(this)->max_length);
		m_Items[index] = value;
	}
	inline uint8_t GetAtUnchecked(il2cpp_array_size_t index) const
	{
		return m_Items[index];
	}
	inline uint8_t* GetAddressAtUnchecked(il2cpp_array_size_t index)
	{
		return m_Items + index;
	}
	inline void SetAtUnchecked(il2cpp_array_size_t index, uint8_t value)
	{
		m_Items[index] = value;
	}
};
struct StringU5BU5D_t7674CD946EC0CE7B3AE0BE70E6EE85F2ECD9F248  : public RuntimeArray
{
	ALIGN_FIELD (8) String_t* m_Items[1];

	inline String_t* GetAt(il2cpp_array_size_t index) const
	{
		IL2CPP_ARRAY_BOUNDS_CHECK(index, (uint32_t)(this)->max_length);
		return m_Items[index];
	}
	inline String_t** GetAddressAt(il2cpp_array_size_t index)
	{
		IL2CPP_ARRAY_BOUNDS_CHECK(index, (uint32_t)(this)->max_length);
		return m_Items + index;
	}
	inline void SetAt(il2cpp_array_size_t index, String_t* value)
	{
		IL2CPP_ARRAY_BOUNDS_CHECK(index, (uint32_t)(this)->max_length);
		m_Items[index] = value;
		Il2CppCodeGenWriteBarrier((void**)m_Items + index, (void*)value);
	}
	inline String_t* GetAtUnchecked(il2cpp_array_size_t index) const
	{
		return m_Items[index];
	}
	inline String_t** GetAddressAtUnchecked(il2cpp_array_size_t index)
	{
		return m_Items + index;
	}
	inline void SetAtUnchecked(il2cpp_array_size_t index, String_t* value)
	{
		m_Items[index] = value;
		Il2CppCodeGenWriteBarrier((void**)m_Items + index, (void*)value);
	}
};
struct ObjectU5BU5D_t8061030B0A12A55D5AD8652A20C922FE99450918  : public RuntimeArray
{
	ALIGN_FIELD (8) RuntimeObject* m_Items[1];

	inline RuntimeObject* GetAt(il2cpp_array_size_t index) const
	{
		IL2CPP_ARRAY_BOUNDS_CHECK(index, (uint32_t)(this)->max_length);
		return m_Items[index];
	}
	inline RuntimeObject** GetAddressAt(il2cpp_array_size_t index)
	{
		IL2CPP_ARRAY_BOUNDS_CHECK(index, (uint32_t)(this)->max_length);
		return m_Items + index;
	}
	inline void SetAt(il2cpp_array_size_t index, RuntimeObject* value)
	{
		IL2CPP_ARRAY_BOUNDS_CHECK(index, (uint32_t)(this)->max_length);
		m_Items[index] = value;
		Il2CppCodeGenWriteBarrier((void**)m_Items + index, (void*)value);
	}
	inline RuntimeObject* GetAtUnchecked(il2cpp_array_size_t index) const
	{
		return m_Items[index];
	}
	inline RuntimeObject** GetAddressAtUnchecked(il2cpp_array_size_t index)
	{
		return m_Items + index;
	}
	inline void SetAtUnchecked(il2cpp_array_size_t index, RuntimeObject* value)
	{
		m_Items[index] = value;
		Il2CppCodeGenWriteBarrier((void**)m_Items + index, (void*)value);
	}
};


IL2CPP_MANAGED_FORCE_INLINE IL2CPP_METHOD_ATTR ObjectU5BU5D_t8061030B0A12A55D5AD8652A20C922FE99450918* Array_Empty_TisRuntimeObject_mFB8A63D602BB6974D31E20300D9EB89C6FE7C278_gshared_inline (const RuntimeMethod* method) ;

IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR void Attribute__ctor_m79ED1BF1EE36D1E417BA89A0D9F91F8AAD8D19E2 (Attribute_tFDA8EFEFB0711976D22474794576DAF28F7440AA* __this, const RuntimeMethod* method) ;
IL2CPP_MANAGED_FORCE_INLINE IL2CPP_METHOD_ATTR bool SR_UsingResourceKeys_m36E934AE31A6845467D5FC45D6139D7CFFBAA0B6_inline (const RuntimeMethod* method) ;
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR ResourceManager_t311D6D32A753224008949B32CC6A5468C47498EB* SR_get_ResourceManager_m49A9F3011AF25967B098181EE135F857F500D9C1 (const RuntimeMethod* method) ;
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR bool String_Equals_mCD5F35DEDCAFE51ACD4E033726FC2EF8DF7E9B4D (String_t* __this, String_t* ___0_value, const RuntimeMethod* method) ;
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR Type_t* Type_GetTypeFromHandle_m6062B81682F79A4D6DF2640692EE6D9987858C57 (RuntimeTypeHandle_t332A452B8B6179E4469B69525D0FE82A88030F7B ___0_handle, const RuntimeMethod* method) ;
IL2CPP_EXTERN_C IL2CPP_NO_INLINE IL2CPP_METHOD_ATTR void ResourceManager__ctor_mC93D478F43E5089ACC407FDECF067A0F208A3784 (ResourceManager_t311D6D32A753224008949B32CC6A5468C47498EB* __this, Type_t* ___0_resourceSource, const RuntimeMethod* method) ;
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR String_t* SR_GetResourceString_m47B3330511C4E4AF493759E9EC538CAB27326576 (String_t* ___0_resourceKey, String_t* ___1_defaultString, const RuntimeMethod* method) ;
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR bool AppContext_TryGetSwitch_mD2500DB32F941228B6964BC53CAA0EA7047AEB78 (String_t* ___0_switchName, bool* ___1_isEnabled, const RuntimeMethod* method) ;
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR void OSPlatformAttribute__ctor_m98E51459152E6F020036239AC09A63800409F20A (OSPlatformAttribute_t7542A2BF18E4C64EED099C1FB8150B228C7EF68B* __this, String_t* ___0_platformName, const RuntimeMethod* method) ;
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR RegistryKey_t8752E3D024B69A6A9CC30AEB0E75F590C74E30D4* RegistryKey_OpenBaseKey_m894C0EE066C153D68657AF07BD3D5C6B4D53FA0D (int32_t ___0_hKey, int32_t ___1_view, const RuntimeMethod* method) ;
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR bool RegistryKey_IsSystemKey_m9E0980A65B2FBD73C34EC940D7572F90AF33D7B6 (RegistryKey_t8752E3D024B69A6A9CC30AEB0E75F590C74E30D4* __this, const RuntimeMethod* method) ;
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR void SafeHandle_Dispose_m4FB5B8A7ED78B90757F1B570D4025F3BA26A39F3 (SafeHandle_tC1A4DA80DA89B867CC011B707A07275230321BF7* __this, const RuntimeMethod* method) ;
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR bool RegistryKey_IsPerfDataKey_mD390E920EE9A798C7822D0E3DF1D149FB7CDB3B3 (RegistryKey_t8752E3D024B69A6A9CC30AEB0E75F590C74E30D4* __this, const RuntimeMethod* method) ;
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR void RegistryKey_ClosePerfDataKey_mA9A7893C2D9C587AD7F750CB3AF290BDAF57EE39 (RegistryKey_t8752E3D024B69A6A9CC30AEB0E75F590C74E30D4* __this, const RuntimeMethod* method) ;
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR void RegistryKey_ValidateKeyView_m6750A1F58ACF19E810049D095CCF0744807879F0 (int32_t ___0_view, const RuntimeMethod* method) ;
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR RegistryKey_t8752E3D024B69A6A9CC30AEB0E75F590C74E30D4* RegistryKey_OpenBaseKeyCore_mC9589E517BD09EBA2BAAFE07DEFA4D5D7D992151 (int32_t ___0_hKey, int32_t ___1_view, const RuntimeMethod* method) ;
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR RegistryKey_t8752E3D024B69A6A9CC30AEB0E75F590C74E30D4* RegistryKey_OpenSubKey_mD3011C61B6D1D74DDB0588A2474C8A169EC52F7F (RegistryKey_t8752E3D024B69A6A9CC30AEB0E75F590C74E30D4* __this, String_t* ___0_name, bool ___1_writable, const RuntimeMethod* method) ;
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR void RegistryKey_ValidateKeyName_m738969C019B38EEA5B8C3C25BB7D4576FE0EE203 (String_t* ___0_name, const RuntimeMethod* method) ;
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR void RegistryKey_EnsureNotDisposed_m8BE19DC7F1E7B9C2123D3BB6416905EFE68AFD78 (RegistryKey_t8752E3D024B69A6A9CC30AEB0E75F590C74E30D4* __this, const RuntimeMethod* method) ;
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR String_t* RegistryKey_FixupName_mEEBBCFD547C00AAEAAA3D54711D12C65C9B4078F (String_t* ___0_name, const RuntimeMethod* method) ;
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR RegistryKey_t8752E3D024B69A6A9CC30AEB0E75F590C74E30D4* RegistryKey_InternalOpenSubKeyCore_mD0430381A6C05276BF333D5FF370C3CED944BFB3 (RegistryKey_t8752E3D024B69A6A9CC30AEB0E75F590C74E30D4* __this, String_t* ___0_name, bool ___1_writable, const RuntimeMethod* method) ;
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR int32_t RegistryKey_InternalSubKeyCountCore_mC707C8439FDE18743C1ADB336C72E30C83B61995 (RegistryKey_t8752E3D024B69A6A9CC30AEB0E75F590C74E30D4* __this, const RuntimeMethod* method) ;
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR int32_t RegistryKey_get_SubKeyCount_mAE3905F230631A9AA8BD138BCF3325F237AD7F24 (RegistryKey_t8752E3D024B69A6A9CC30AEB0E75F590C74E30D4* __this, const RuntimeMethod* method) ;
inline StringU5BU5D_t7674CD946EC0CE7B3AE0BE70E6EE85F2ECD9F248* Array_Empty_TisString_t_m9832B70DF2B936246FE60F75D3D12CB946C39D16_inline (const RuntimeMethod* method)
{
	return ((  StringU5BU5D_t7674CD946EC0CE7B3AE0BE70E6EE85F2ECD9F248* (*) (const RuntimeMethod*))Array_Empty_TisRuntimeObject_mFB8A63D602BB6974D31E20300D9EB89C6FE7C278_gshared_inline)(method);
}
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR StringU5BU5D_t7674CD946EC0CE7B3AE0BE70E6EE85F2ECD9F248* RegistryKey_InternalGetSubKeyNamesCore_m715FD9D02D73A1BE3FA48509AB2378A1880A798A (RegistryKey_t8752E3D024B69A6A9CC30AEB0E75F590C74E30D4* __this, int32_t ___0_subkeys, const RuntimeMethod* method) ;
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR RuntimeObject* RegistryKey_InternalGetValue_m37B31B8390738F2E98F6138DD12FB86CEECC1ADB (RegistryKey_t8752E3D024B69A6A9CC30AEB0E75F590C74E30D4* __this, String_t* ___0_name, RuntimeObject* ___1_defaultValue, bool ___2_doNotExpand, const RuntimeMethod* method) ;
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR RuntimeObject* RegistryKey_InternalGetValueCore_m139A3171C748AAD22E972CB0048FD87655B00ACA (RegistryKey_t8752E3D024B69A6A9CC30AEB0E75F590C74E30D4* __this, String_t* ___0_name, RuntimeObject* ___1_defaultValue, bool ___2_doNotExpand, const RuntimeMethod* method) ;
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR int32_t String_IndexOf_mE21E78F35EF4A7768E385A72814C88D22B689966 (String_t* __this, Il2CppChar ___0_value, const RuntimeMethod* method) ;
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR void StringBuilder__ctor_mCD797D942316CB356205FD96415B0B7581CDAD60 (StringBuilder_t* __this, String_t* ___0_value, const RuntimeMethod* method) ;
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR void RegistryKey_FixupPath_m565CE6B665A2A4371F135478D9B19CC22806691A (StringBuilder_t* ___0_path, const RuntimeMethod* method) ;
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR int32_t StringBuilder_get_Length_mDEA041E7357C68CC3B5885276BB403676DAAE0D8 (StringBuilder_t* __this, const RuntimeMethod* method) ;
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR Il2CppChar StringBuilder_get_Chars_m254FD6F2F75C00B0D353D73B2A4A19316BD7624D (StringBuilder_t* __this, int32_t ___0_index, const RuntimeMethod* method) ;
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR void StringBuilder_set_Length_mE2427BDAEF91C4E4A6C80F3BDF1F6E01DBCC2414 (StringBuilder_t* __this, int32_t ___0_value, const RuntimeMethod* method) ;
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR void StringBuilder_set_Chars_m20B53B0EEAB2A0BB0EC84A130FF12EA86ADD99AE (StringBuilder_t* __this, int32_t ___0_index, Il2CppChar ___1_value, const RuntimeMethod* method) ;
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR String_t* SR_get_ObjectDisposed_RegKeyClosed_mFB336460D1656E0787CC0C825DEEAA98AD2F59F6 (const RuntimeMethod* method) ;
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR void ObjectDisposedException__ctor_m5C356C25295E89559C120CB4562783AAF7F41C84 (ObjectDisposedException_tC5FB29E8E980E2010A2F6A5B9B791089419F89EB* __this, String_t* ___0_objectName, String_t* ___1_message, const RuntimeMethod* method) ;
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR void ArgumentNullException__ctor_m444AE141157E333844FC1A9500224C2F9FD24F4B (ArgumentNullException_t327031E412FAB2351B0022DD5DAD47E67E597129* __this, String_t* ___0_paramName, const RuntimeMethod* method) ;
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR int32_t String_IndexOf_m490810CB7ADA9230AC0F8D78E213A8EFED129F55 (String_t* __this, String_t* ___0_value, int32_t ___1_comparisonType, const RuntimeMethod* method) ;
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR String_t* SR_get_Arg_RegKeyStrLenBug_m3A198859EF9C55F4D81EC9E1A9985531D8F0C44F (const RuntimeMethod* method) ;
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR void ArgumentException__ctor_m8F9D40CE19D19B698A70F9A258640EB52DB39B62 (ArgumentException_tAD90411542A20A9C72D5CDA3A84181D8B947A263* __this, String_t* ___0_message, String_t* ___1_paramName, const RuntimeMethod* method) ;
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR int32_t String_IndexOf_m2DFDE7BD37585BDBCD6F688B4E4A93304235A0B8 (String_t* __this, String_t* ___0_value, int32_t ___1_startIndex, int32_t ___2_comparisonType, const RuntimeMethod* method) ;
IL2CPP_MANAGED_FORCE_INLINE IL2CPP_METHOD_ATTR int32_t String_get_Length_m42625D67623FA5CC7A44D47425CE86FB946542D2_inline (String_t* __this, const RuntimeMethod* method) ;
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR String_t* SR_get_Argument_InvalidRegistryViewCheck_m0CBA72BB49ACC26AB5AFF62EF8E1874C9EABD2CF (const RuntimeMethod* method) ;
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR String_t* SR_get_PlatformNotSupported_Registry_m5C3864E998C39F3B499E6155FC86D40B96A13B68 (const RuntimeMethod* method) ;
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR void PlatformNotSupportedException__ctor_mC5103EE3FE4FE245039B1107D6685296D9CC6560 (PlatformNotSupportedException_tD2BD7EB9278518AA5FE8AE75AD5D0D4298A4631A* __this, String_t* ___0_message, const RuntimeMethod* method) ;
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR void IntPtr__ctor_m20A566609A091311C734617C699E61F545250AC7 (intptr_t* __this, int32_t ___0_value, const RuntimeMethod* method) ;
#ifdef __clang__
#pragma clang diagnostic push
#pragma clang diagnostic ignored "-Winvalid-offsetof"
#pragma clang diagnostic ignored "-Wunused-variable"
#endif
#ifdef __clang__
#pragma clang diagnostic pop
#endif
#ifdef __clang__
#pragma clang diagnostic push
#pragma clang diagnostic ignored "-Winvalid-offsetof"
#pragma clang diagnostic ignored "-Wunused-variable"
#endif
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR void EmbeddedAttribute__ctor_mFA429316B809077FF54469C2E5EE8D2EF20F254B (EmbeddedAttribute_tEEA5A57B9AF7201C983759F50BEE83FFC2EC27EC* __this, const RuntimeMethod* method) 
{
	{
		Attribute__ctor_m79ED1BF1EE36D1E417BA89A0D9F91F8AAD8D19E2(__this, NULL);
		return;
	}
}
#ifdef __clang__
#pragma clang diagnostic pop
#endif
#ifdef __clang__
#pragma clang diagnostic push
#pragma clang diagnostic ignored "-Winvalid-offsetof"
#pragma clang diagnostic ignored "-Wunused-variable"
#endif
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR void NullableAttribute__ctor_m68EBE87B4D808A3EC59C38553DC639EB44F84D08 (NullableAttribute_tBE95F229CD35EA5CFBC09238521069226F6805AF* __this, uint8_t ___0_p, const RuntimeMethod* method) 
{
	static bool s_Il2CppMethodInitialized;
	if (!s_Il2CppMethodInitialized)
	{
		il2cpp_codegen_initialize_runtime_metadata((uintptr_t*)&ByteU5BU5D_tA6237BF417AE52AD70CFB4EF24A7A82613DF9031_il2cpp_TypeInfo_var);
		s_Il2CppMethodInitialized = true;
	}
	{
		Attribute__ctor_m79ED1BF1EE36D1E417BA89A0D9F91F8AAD8D19E2(__this, NULL);
		ByteU5BU5D_tA6237BF417AE52AD70CFB4EF24A7A82613DF9031* L_0 = (ByteU5BU5D_tA6237BF417AE52AD70CFB4EF24A7A82613DF9031*)(ByteU5BU5D_tA6237BF417AE52AD70CFB4EF24A7A82613DF9031*)SZArrayNew(ByteU5BU5D_tA6237BF417AE52AD70CFB4EF24A7A82613DF9031_il2cpp_TypeInfo_var, (uint32_t)1);
		ByteU5BU5D_tA6237BF417AE52AD70CFB4EF24A7A82613DF9031* L_1 = L_0;
		uint8_t L_2 = ___0_p;
		NullCheck(L_1);
		(L_1)->SetAt(static_cast<il2cpp_array_size_t>(0), (uint8_t)L_2);
		__this->___NullableFlags = L_1;
		Il2CppCodeGenWriteBarrier((void**)(&__this->___NullableFlags), (void*)L_1);
		return;
	}
}
#ifdef __clang__
#pragma clang diagnostic pop
#endif
#ifdef __clang__
#pragma clang diagnostic push
#pragma clang diagnostic ignored "-Winvalid-offsetof"
#pragma clang diagnostic ignored "-Wunused-variable"
#endif
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR void NullableContextAttribute__ctor_mAA6E40028D0F7BAD8E7F5BE81DF20D425A5B04AC (NullableContextAttribute_t243FAEC75CCC2C5D4BF17411FFE0C81F640A2606* __this, uint8_t ___0_p, const RuntimeMethod* method) 
{
	{
		Attribute__ctor_m79ED1BF1EE36D1E417BA89A0D9F91F8AAD8D19E2(__this, NULL);
		uint8_t L_0 = ___0_p;
		__this->___Flag = L_0;
		return;
	}
}
#ifdef __clang__
#pragma clang diagnostic pop
#endif
#ifdef __clang__
#pragma clang diagnostic push
#pragma clang diagnostic ignored "-Winvalid-offsetof"
#pragma clang diagnostic ignored "-Wunused-variable"
#endif
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR void NullablePublicOnlyAttribute__ctor_m07DEB39F633B46C61114D3790379F321DE46A1CD (NullablePublicOnlyAttribute_t3BF10572C5F45E4F2CD63F7938BAE17FD740114C* __this, bool ___0_p, const RuntimeMethod* method) 
{
	{
		Attribute__ctor_m79ED1BF1EE36D1E417BA89A0D9F91F8AAD8D19E2(__this, NULL);
		bool L_0 = ___0_p;
		__this->___IncludesInternals = L_0;
		return;
	}
}
#ifdef __clang__
#pragma clang diagnostic pop
#endif
#ifdef __clang__
#pragma clang diagnostic push
#pragma clang diagnostic ignored "-Winvalid-offsetof"
#pragma clang diagnostic ignored "-Wunused-variable"
#endif
#ifdef __clang__
#pragma clang diagnostic pop
#endif
#ifdef __clang__
#pragma clang diagnostic push
#pragma clang diagnostic ignored "-Winvalid-offsetof"
#pragma clang diagnostic ignored "-Wunused-variable"
#endif
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR bool SR_UsingResourceKeys_m36E934AE31A6845467D5FC45D6139D7CFFBAA0B6 (const RuntimeMethod* method) 
{
	static bool s_Il2CppMethodInitialized;
	if (!s_Il2CppMethodInitialized)
	{
		il2cpp_codegen_initialize_runtime_metadata((uintptr_t*)&SR_tCF5D02DC363D3707E4B0700773B397B107D749CF_il2cpp_TypeInfo_var);
		s_Il2CppMethodInitialized = true;
	}
	{
		il2cpp_codegen_runtime_class_init_inline(SR_tCF5D02DC363D3707E4B0700773B397B107D749CF_il2cpp_TypeInfo_var);
		bool L_0 = ((SR_tCF5D02DC363D3707E4B0700773B397B107D749CF_StaticFields*)il2cpp_codegen_static_fields_for(SR_tCF5D02DC363D3707E4B0700773B397B107D749CF_il2cpp_TypeInfo_var))->___s_usingResourceKeys;
		return L_0;
	}
}
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR String_t* SR_GetResourceString_m47B3330511C4E4AF493759E9EC538CAB27326576 (String_t* ___0_resourceKey, String_t* ___1_defaultString, const RuntimeMethod* method) 
{
	static bool s_Il2CppMethodInitialized;
	if (!s_Il2CppMethodInitialized)
	{
		il2cpp_codegen_initialize_runtime_metadata((uintptr_t*)&SR_tCF5D02DC363D3707E4B0700773B397B107D749CF_il2cpp_TypeInfo_var);
		s_Il2CppMethodInitialized = true;
	}
	String_t* V_0 = NULL;
	il2cpp::utils::ExceptionSupportStack<RuntimeObject*, 1> __active_exceptions;
	String_t* G_B3_0 = NULL;
	String_t* G_B2_0 = NULL;
	{
		il2cpp_codegen_runtime_class_init_inline(SR_tCF5D02DC363D3707E4B0700773B397B107D749CF_il2cpp_TypeInfo_var);
		bool L_0;
		L_0 = SR_UsingResourceKeys_m36E934AE31A6845467D5FC45D6139D7CFFBAA0B6_inline(NULL);
		if (!L_0)
		{
			goto IL_000e;
		}
	}
	{
		String_t* L_1 = ___1_defaultString;
		String_t* L_2 = L_1;
		if (L_2)
		{
			G_B3_0 = L_2;
			goto IL_000d;
		}
		G_B2_0 = L_2;
	}
	{
		String_t* L_3 = ___0_resourceKey;
		G_B3_0 = L_3;
	}

IL_000d:
	{
		return G_B3_0;
	}

IL_000e:
	{
		V_0 = (String_t*)NULL;
	}
	try
	{
		il2cpp_codegen_runtime_class_init_inline(SR_tCF5D02DC363D3707E4B0700773B397B107D749CF_il2cpp_TypeInfo_var);
		ResourceManager_t311D6D32A753224008949B32CC6A5468C47498EB* L_4;
		L_4 = SR_get_ResourceManager_m49A9F3011AF25967B098181EE135F857F500D9C1(NULL);
		String_t* L_5 = ___0_resourceKey;
		NullCheck(L_4);
		String_t* L_6;
		L_6 = VirtualFuncInvoker1< String_t*, String_t* >::Invoke(7, L_4, L_5);
		V_0 = L_6;
		goto IL_0021;
	}
	catch(Il2CppExceptionWrapper& e)
	{
		if(il2cpp_codegen_class_is_assignable_from (((RuntimeClass*)il2cpp_codegen_initialize_runtime_metadata_inline((uintptr_t*)&MissingManifestResourceException_t136A089345909ADB6333D6F4E2AA84C7A00CB3FD_il2cpp_TypeInfo_var)), il2cpp_codegen_object_class(e.ex)))
		{
			IL2CPP_PUSH_ACTIVE_EXCEPTION(e.ex);
			goto CATCH_001e;
		}
		throw e;
	}

CATCH_001e:
	{
		MissingManifestResourceException_t136A089345909ADB6333D6F4E2AA84C7A00CB3FD* L_7 = ((MissingManifestResourceException_t136A089345909ADB6333D6F4E2AA84C7A00CB3FD*)IL2CPP_GET_ACTIVE_EXCEPTION(MissingManifestResourceException_t136A089345909ADB6333D6F4E2AA84C7A00CB3FD*));;
		IL2CPP_POP_ACTIVE_EXCEPTION(Exception_t*);
		goto IL_0021;
	}

IL_0021:
	{
		String_t* L_8 = ___1_defaultString;
		if (!L_8)
		{
			goto IL_002f;
		}
	}
	{
		String_t* L_9 = ___0_resourceKey;
		String_t* L_10 = V_0;
		NullCheck(L_9);
		bool L_11;
		L_11 = String_Equals_mCD5F35DEDCAFE51ACD4E033726FC2EF8DF7E9B4D(L_9, L_10, NULL);
		if (!L_11)
		{
			goto IL_002f;
		}
	}
	{
		String_t* L_12 = ___1_defaultString;
		return L_12;
	}

IL_002f:
	{
		String_t* L_13 = V_0;
		return L_13;
	}
}
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR ResourceManager_t311D6D32A753224008949B32CC6A5468C47498EB* SR_get_ResourceManager_m49A9F3011AF25967B098181EE135F857F500D9C1 (const RuntimeMethod* method) 
{
	static bool s_Il2CppMethodInitialized;
	if (!s_Il2CppMethodInitialized)
	{
		il2cpp_codegen_initialize_runtime_metadata((uintptr_t*)&ResourceManager_t311D6D32A753224008949B32CC6A5468C47498EB_il2cpp_TypeInfo_var);
		il2cpp_codegen_initialize_runtime_metadata((uintptr_t*)&SR_t17E2A436BA997C55733E1B4C195401F61766CEF9_0_0_0_var);
		il2cpp_codegen_initialize_runtime_metadata((uintptr_t*)&SR_tCF5D02DC363D3707E4B0700773B397B107D749CF_il2cpp_TypeInfo_var);
		il2cpp_codegen_initialize_runtime_metadata((uintptr_t*)&Type_t_il2cpp_TypeInfo_var);
		s_Il2CppMethodInitialized = true;
	}
	ResourceManager_t311D6D32A753224008949B32CC6A5468C47498EB* G_B2_0 = NULL;
	ResourceManager_t311D6D32A753224008949B32CC6A5468C47498EB* G_B1_0 = NULL;
	{
		il2cpp_codegen_runtime_class_init_inline(SR_tCF5D02DC363D3707E4B0700773B397B107D749CF_il2cpp_TypeInfo_var);
		ResourceManager_t311D6D32A753224008949B32CC6A5468C47498EB* L_0 = ((SR_tCF5D02DC363D3707E4B0700773B397B107D749CF_StaticFields*)il2cpp_codegen_static_fields_for(SR_tCF5D02DC363D3707E4B0700773B397B107D749CF_il2cpp_TypeInfo_var))->___s_resourceManager;
		ResourceManager_t311D6D32A753224008949B32CC6A5468C47498EB* L_1 = L_0;
		if (L_1)
		{
			G_B2_0 = L_1;
			goto IL_001e;
		}
		G_B1_0 = L_1;
	}
	{
		RuntimeTypeHandle_t332A452B8B6179E4469B69525D0FE82A88030F7B L_2 = { reinterpret_cast<intptr_t> (SR_t17E2A436BA997C55733E1B4C195401F61766CEF9_0_0_0_var) };
		il2cpp_codegen_runtime_class_init_inline(Type_t_il2cpp_TypeInfo_var);
		Type_t* L_3;
		L_3 = Type_GetTypeFromHandle_m6062B81682F79A4D6DF2640692EE6D9987858C57(L_2, NULL);
		ResourceManager_t311D6D32A753224008949B32CC6A5468C47498EB* L_4 = (ResourceManager_t311D6D32A753224008949B32CC6A5468C47498EB*)il2cpp_codegen_object_new(ResourceManager_t311D6D32A753224008949B32CC6A5468C47498EB_il2cpp_TypeInfo_var);
		ResourceManager__ctor_mC93D478F43E5089ACC407FDECF067A0F208A3784(L_4, L_3, NULL);
		ResourceManager_t311D6D32A753224008949B32CC6A5468C47498EB* L_5 = L_4;
		il2cpp_codegen_runtime_class_init_inline(SR_tCF5D02DC363D3707E4B0700773B397B107D749CF_il2cpp_TypeInfo_var);
		((SR_tCF5D02DC363D3707E4B0700773B397B107D749CF_StaticFields*)il2cpp_codegen_static_fields_for(SR_tCF5D02DC363D3707E4B0700773B397B107D749CF_il2cpp_TypeInfo_var))->___s_resourceManager = L_5;
		Il2CppCodeGenWriteBarrier((void**)(&((SR_tCF5D02DC363D3707E4B0700773B397B107D749CF_StaticFields*)il2cpp_codegen_static_fields_for(SR_tCF5D02DC363D3707E4B0700773B397B107D749CF_il2cpp_TypeInfo_var))->___s_resourceManager), (void*)L_5);
		G_B2_0 = L_5;
	}

IL_001e:
	{
		return G_B2_0;
	}
}
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR String_t* SR_get_Arg_RegKeyStrLenBug_m3A198859EF9C55F4D81EC9E1A9985531D8F0C44F (const RuntimeMethod* method) 
{
	static bool s_Il2CppMethodInitialized;
	if (!s_Il2CppMethodInitialized)
	{
		il2cpp_codegen_initialize_runtime_metadata((uintptr_t*)&SR_tCF5D02DC363D3707E4B0700773B397B107D749CF_il2cpp_TypeInfo_var);
		il2cpp_codegen_initialize_runtime_metadata((uintptr_t*)&_stringLiteralA0324410B1B7CC964150BD7DC521C17C3491F407);
		s_Il2CppMethodInitialized = true;
	}
	{
		il2cpp_codegen_runtime_class_init_inline(SR_tCF5D02DC363D3707E4B0700773B397B107D749CF_il2cpp_TypeInfo_var);
		String_t* L_0;
		L_0 = SR_GetResourceString_m47B3330511C4E4AF493759E9EC538CAB27326576(_stringLiteralA0324410B1B7CC964150BD7DC521C17C3491F407, (String_t*)NULL, NULL);
		return L_0;
	}
}
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR String_t* SR_get_Argument_InvalidRegistryViewCheck_m0CBA72BB49ACC26AB5AFF62EF8E1874C9EABD2CF (const RuntimeMethod* method) 
{
	static bool s_Il2CppMethodInitialized;
	if (!s_Il2CppMethodInitialized)
	{
		il2cpp_codegen_initialize_runtime_metadata((uintptr_t*)&SR_tCF5D02DC363D3707E4B0700773B397B107D749CF_il2cpp_TypeInfo_var);
		il2cpp_codegen_initialize_runtime_metadata((uintptr_t*)&_stringLiteral21E1B4354893A602D3EAE4BBA57311B083D8E0A0);
		s_Il2CppMethodInitialized = true;
	}
	{
		il2cpp_codegen_runtime_class_init_inline(SR_tCF5D02DC363D3707E4B0700773B397B107D749CF_il2cpp_TypeInfo_var);
		String_t* L_0;
		L_0 = SR_GetResourceString_m47B3330511C4E4AF493759E9EC538CAB27326576(_stringLiteral21E1B4354893A602D3EAE4BBA57311B083D8E0A0, (String_t*)NULL, NULL);
		return L_0;
	}
}
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR String_t* SR_get_ObjectDisposed_RegKeyClosed_mFB336460D1656E0787CC0C825DEEAA98AD2F59F6 (const RuntimeMethod* method) 
{
	static bool s_Il2CppMethodInitialized;
	if (!s_Il2CppMethodInitialized)
	{
		il2cpp_codegen_initialize_runtime_metadata((uintptr_t*)&SR_tCF5D02DC363D3707E4B0700773B397B107D749CF_il2cpp_TypeInfo_var);
		il2cpp_codegen_initialize_runtime_metadata((uintptr_t*)&_stringLiteralCD83C95652A081F6D88BF9BB6415F31CED9C449E);
		s_Il2CppMethodInitialized = true;
	}
	{
		il2cpp_codegen_runtime_class_init_inline(SR_tCF5D02DC363D3707E4B0700773B397B107D749CF_il2cpp_TypeInfo_var);
		String_t* L_0;
		L_0 = SR_GetResourceString_m47B3330511C4E4AF493759E9EC538CAB27326576(_stringLiteralCD83C95652A081F6D88BF9BB6415F31CED9C449E, (String_t*)NULL, NULL);
		return L_0;
	}
}
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR String_t* SR_get_PlatformNotSupported_Registry_m5C3864E998C39F3B499E6155FC86D40B96A13B68 (const RuntimeMethod* method) 
{
	static bool s_Il2CppMethodInitialized;
	if (!s_Il2CppMethodInitialized)
	{
		il2cpp_codegen_initialize_runtime_metadata((uintptr_t*)&SR_tCF5D02DC363D3707E4B0700773B397B107D749CF_il2cpp_TypeInfo_var);
		il2cpp_codegen_initialize_runtime_metadata((uintptr_t*)&_stringLiteral592A968CA63418B09EC2CE77B5A94E7EC7109F1F);
		s_Il2CppMethodInitialized = true;
	}
	{
		il2cpp_codegen_runtime_class_init_inline(SR_tCF5D02DC363D3707E4B0700773B397B107D749CF_il2cpp_TypeInfo_var);
		String_t* L_0;
		L_0 = SR_GetResourceString_m47B3330511C4E4AF493759E9EC538CAB27326576(_stringLiteral592A968CA63418B09EC2CE77B5A94E7EC7109F1F, (String_t*)NULL, NULL);
		return L_0;
	}
}
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR void SR__cctor_m135A729A10AAF93B9605DFD6A23BD55EF0F6BEF3 (const RuntimeMethod* method) 
{
	static bool s_Il2CppMethodInitialized;
	if (!s_Il2CppMethodInitialized)
	{
		il2cpp_codegen_initialize_runtime_metadata((uintptr_t*)&AppContext_t0380D19FAC72CD59D46947D86DC1DAA3BCE638E0_il2cpp_TypeInfo_var);
		il2cpp_codegen_initialize_runtime_metadata((uintptr_t*)&SR_tCF5D02DC363D3707E4B0700773B397B107D749CF_il2cpp_TypeInfo_var);
		il2cpp_codegen_initialize_runtime_metadata((uintptr_t*)&_stringLiteralB5528CA07A43AEA4EFA2F7B2DEF38E0A5D87ECD6);
		s_Il2CppMethodInitialized = true;
	}
	bool V_0 = false;
	int32_t G_B3_0 = 0;
	{
		il2cpp_codegen_runtime_class_init_inline(AppContext_t0380D19FAC72CD59D46947D86DC1DAA3BCE638E0_il2cpp_TypeInfo_var);
		bool L_0;
		L_0 = AppContext_TryGetSwitch_mD2500DB32F941228B6964BC53CAA0EA7047AEB78(_stringLiteralB5528CA07A43AEA4EFA2F7B2DEF38E0A5D87ECD6, (&V_0), NULL);
		if (L_0)
		{
			goto IL_0011;
		}
	}
	{
		G_B3_0 = 0;
		goto IL_0012;
	}

IL_0011:
	{
		bool L_1 = V_0;
		G_B3_0 = ((int32_t)(L_1));
	}

IL_0012:
	{
		((SR_tCF5D02DC363D3707E4B0700773B397B107D749CF_StaticFields*)il2cpp_codegen_static_fields_for(SR_tCF5D02DC363D3707E4B0700773B397B107D749CF_il2cpp_TypeInfo_var))->___s_usingResourceKeys = (bool)G_B3_0;
		return;
	}
}
#ifdef __clang__
#pragma clang diagnostic pop
#endif
#ifdef __clang__
#pragma clang diagnostic push
#pragma clang diagnostic ignored "-Winvalid-offsetof"
#pragma clang diagnostic ignored "-Wunused-variable"
#endif
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR void OSPlatformAttribute__ctor_m98E51459152E6F020036239AC09A63800409F20A (OSPlatformAttribute_t7542A2BF18E4C64EED099C1FB8150B228C7EF68B* __this, String_t* ___0_platformName, const RuntimeMethod* method) 
{
	{
		Attribute__ctor_m79ED1BF1EE36D1E417BA89A0D9F91F8AAD8D19E2(__this, NULL);
		String_t* L_0 = ___0_platformName;
		__this->___U3CPlatformNameU3Ek__BackingField = L_0;
		Il2CppCodeGenWriteBarrier((void**)(&__this->___U3CPlatformNameU3Ek__BackingField), (void*)L_0);
		return;
	}
}
#ifdef __clang__
#pragma clang diagnostic pop
#endif
#ifdef __clang__
#pragma clang diagnostic push
#pragma clang diagnostic ignored "-Winvalid-offsetof"
#pragma clang diagnostic ignored "-Wunused-variable"
#endif
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR void SupportedOSPlatformAttribute__ctor_mED8B23540D954F666DD748D77FC341A97D26BC91 (SupportedOSPlatformAttribute_t5154EE63A81CFEFDCFDAC80983E85D33825F7C2C* __this, String_t* ___0_platformName, const RuntimeMethod* method) 
{
	{
		String_t* L_0 = ___0_platformName;
		OSPlatformAttribute__ctor_m98E51459152E6F020036239AC09A63800409F20A(__this, L_0, NULL);
		return;
	}
}
#ifdef __clang__
#pragma clang diagnostic pop
#endif
#ifdef __clang__
#pragma clang diagnostic push
#pragma clang diagnostic ignored "-Winvalid-offsetof"
#pragma clang diagnostic ignored "-Wunused-variable"
#endif
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR void Registry__cctor_mEF6C8513B7F0DDDC73206847C9D61FB1F49ED817 (const RuntimeMethod* method) 
{
	static bool s_Il2CppMethodInitialized;
	if (!s_Il2CppMethodInitialized)
	{
		il2cpp_codegen_initialize_runtime_metadata((uintptr_t*)&RegistryKey_t8752E3D024B69A6A9CC30AEB0E75F590C74E30D4_il2cpp_TypeInfo_var);
		il2cpp_codegen_initialize_runtime_metadata((uintptr_t*)&Registry_tE2BC25EC64E312939C45900E01D9B533CBE70024_il2cpp_TypeInfo_var);
		s_Il2CppMethodInitialized = true;
	}
	{
		il2cpp_codegen_runtime_class_init_inline(RegistryKey_t8752E3D024B69A6A9CC30AEB0E75F590C74E30D4_il2cpp_TypeInfo_var);
		RegistryKey_t8752E3D024B69A6A9CC30AEB0E75F590C74E30D4* L_0;
		L_0 = RegistryKey_OpenBaseKey_m894C0EE066C153D68657AF07BD3D5C6B4D53FA0D(((int32_t)-2147483647), 0, NULL);
		((Registry_tE2BC25EC64E312939C45900E01D9B533CBE70024_StaticFields*)il2cpp_codegen_static_fields_for(Registry_tE2BC25EC64E312939C45900E01D9B533CBE70024_il2cpp_TypeInfo_var))->___CurrentUser = L_0;
		Il2CppCodeGenWriteBarrier((void**)(&((Registry_tE2BC25EC64E312939C45900E01D9B533CBE70024_StaticFields*)il2cpp_codegen_static_fields_for(Registry_tE2BC25EC64E312939C45900E01D9B533CBE70024_il2cpp_TypeInfo_var))->___CurrentUser), (void*)L_0);
		RegistryKey_t8752E3D024B69A6A9CC30AEB0E75F590C74E30D4* L_1;
		L_1 = RegistryKey_OpenBaseKey_m894C0EE066C153D68657AF07BD3D5C6B4D53FA0D(((int32_t)-2147483646), 0, NULL);
		((Registry_tE2BC25EC64E312939C45900E01D9B533CBE70024_StaticFields*)il2cpp_codegen_static_fields_for(Registry_tE2BC25EC64E312939C45900E01D9B533CBE70024_il2cpp_TypeInfo_var))->___LocalMachine = L_1;
		Il2CppCodeGenWriteBarrier((void**)(&((Registry_tE2BC25EC64E312939C45900E01D9B533CBE70024_StaticFields*)il2cpp_codegen_static_fields_for(Registry_tE2BC25EC64E312939C45900E01D9B533CBE70024_il2cpp_TypeInfo_var))->___LocalMachine), (void*)L_1);
		RegistryKey_t8752E3D024B69A6A9CC30AEB0E75F590C74E30D4* L_2;
		L_2 = RegistryKey_OpenBaseKey_m894C0EE066C153D68657AF07BD3D5C6B4D53FA0D(((int32_t)-2147483648LL), 0, NULL);
		((Registry_tE2BC25EC64E312939C45900E01D9B533CBE70024_StaticFields*)il2cpp_codegen_static_fields_for(Registry_tE2BC25EC64E312939C45900E01D9B533CBE70024_il2cpp_TypeInfo_var))->___ClassesRoot = L_2;
		Il2CppCodeGenWriteBarrier((void**)(&((Registry_tE2BC25EC64E312939C45900E01D9B533CBE70024_StaticFields*)il2cpp_codegen_static_fields_for(Registry_tE2BC25EC64E312939C45900E01D9B533CBE70024_il2cpp_TypeInfo_var))->___ClassesRoot), (void*)L_2);
		RegistryKey_t8752E3D024B69A6A9CC30AEB0E75F590C74E30D4* L_3;
		L_3 = RegistryKey_OpenBaseKey_m894C0EE066C153D68657AF07BD3D5C6B4D53FA0D(((int32_t)-2147483645), 0, NULL);
		((Registry_tE2BC25EC64E312939C45900E01D9B533CBE70024_StaticFields*)il2cpp_codegen_static_fields_for(Registry_tE2BC25EC64E312939C45900E01D9B533CBE70024_il2cpp_TypeInfo_var))->___Users = L_3;
		Il2CppCodeGenWriteBarrier((void**)(&((Registry_tE2BC25EC64E312939C45900E01D9B533CBE70024_StaticFields*)il2cpp_codegen_static_fields_for(Registry_tE2BC25EC64E312939C45900E01D9B533CBE70024_il2cpp_TypeInfo_var))->___Users), (void*)L_3);
		RegistryKey_t8752E3D024B69A6A9CC30AEB0E75F590C74E30D4* L_4;
		L_4 = RegistryKey_OpenBaseKey_m894C0EE066C153D68657AF07BD3D5C6B4D53FA0D(((int32_t)-2147483644), 0, NULL);
		((Registry_tE2BC25EC64E312939C45900E01D9B533CBE70024_StaticFields*)il2cpp_codegen_static_fields_for(Registry_tE2BC25EC64E312939C45900E01D9B533CBE70024_il2cpp_TypeInfo_var))->___PerformanceData = L_4;
		Il2CppCodeGenWriteBarrier((void**)(&((Registry_tE2BC25EC64E312939C45900E01D9B533CBE70024_StaticFields*)il2cpp_codegen_static_fields_for(Registry_tE2BC25EC64E312939C45900E01D9B533CBE70024_il2cpp_TypeInfo_var))->___PerformanceData), (void*)L_4);
		RegistryKey_t8752E3D024B69A6A9CC30AEB0E75F590C74E30D4* L_5;
		L_5 = RegistryKey_OpenBaseKey_m894C0EE066C153D68657AF07BD3D5C6B4D53FA0D(((int32_t)-2147483643), 0, NULL);
		((Registry_tE2BC25EC64E312939C45900E01D9B533CBE70024_StaticFields*)il2cpp_codegen_static_fields_for(Registry_tE2BC25EC64E312939C45900E01D9B533CBE70024_il2cpp_TypeInfo_var))->___CurrentConfig = L_5;
		Il2CppCodeGenWriteBarrier((void**)(&((Registry_tE2BC25EC64E312939C45900E01D9B533CBE70024_StaticFields*)il2cpp_codegen_static_fields_for(Registry_tE2BC25EC64E312939C45900E01D9B533CBE70024_il2cpp_TypeInfo_var))->___CurrentConfig), (void*)L_5);
		return;
	}
}
#ifdef __clang__
#pragma clang diagnostic pop
#endif
#ifdef __clang__
#pragma clang diagnostic push
#pragma clang diagnostic ignored "-Winvalid-offsetof"
#pragma clang diagnostic ignored "-Wunused-variable"
#endif
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR void RegistryKey_Dispose_m30EFEE96E488FA3E1DB21829658110F799225D66 (RegistryKey_t8752E3D024B69A6A9CC30AEB0E75F590C74E30D4* __this, const RuntimeMethod* method) 
{
	il2cpp::utils::ExceptionSupportStack<RuntimeObject*, 1> __active_exceptions;
	{
		SafeRegistryHandle_t890BD43C81043709A2103F1FDC4394C603EE1FDF* L_0 = __this->____hkey;
		il2cpp_codegen_memory_barrier();
		if (!L_0)
		{
			goto IL_003c;
		}
	}
	{
		bool L_1;
		L_1 = RegistryKey_IsSystemKey_m9E0980A65B2FBD73C34EC940D7572F90AF33D7B6(__this, NULL);
		if (L_1)
		{
			goto IL_002e;
		}
	}
	{
		auto __finallyBlock = il2cpp::utils::Finally([&]
		{

FINALLY_0024:
			{
				il2cpp_codegen_memory_barrier();
				__this->____hkey = (SafeRegistryHandle_t890BD43C81043709A2103F1FDC4394C603EE1FDF*)NULL;
				Il2CppCodeGenWriteBarrier((void**)(&__this->____hkey), (void*)(SafeRegistryHandle_t890BD43C81043709A2103F1FDC4394C603EE1FDF*)NULL);
				return;
			}
		});
		try
		{
			try
			{
				SafeRegistryHandle_t890BD43C81043709A2103F1FDC4394C603EE1FDF* L_2 = __this->____hkey;
				il2cpp_codegen_memory_barrier();
				NullCheck(L_2);
				SafeHandle_Dispose_m4FB5B8A7ED78B90757F1B570D4025F3BA26A39F3(L_2, NULL);
				goto IL_003c;
			}
			catch(Il2CppExceptionWrapper& e)
			{
				if(il2cpp_codegen_class_is_assignable_from (((RuntimeClass*)il2cpp_codegen_initialize_runtime_metadata_inline((uintptr_t*)&IOException_t5D599190B003D41D45D4839A9B6B9AB53A755910_il2cpp_TypeInfo_var)), il2cpp_codegen_object_class(e.ex)))
				{
					IL2CPP_PUSH_ACTIVE_EXCEPTION(e.ex);
					goto CATCH_0021_1;
				}
				throw e;
			}

CATCH_0021_1:
			{
				IOException_t5D599190B003D41D45D4839A9B6B9AB53A755910* L_3 = ((IOException_t5D599190B003D41D45D4839A9B6B9AB53A755910*)IL2CPP_GET_ACTIVE_EXCEPTION(IOException_t5D599190B003D41D45D4839A9B6B9AB53A755910*));;
				IL2CPP_POP_ACTIVE_EXCEPTION(Exception_t*);
				goto IL_003c;
			}
		}
		catch(Il2CppExceptionWrapper& e)
		{
			__finallyBlock.StoreException(e.ex);
		}
	}

IL_002e:
	{
		bool L_4;
		L_4 = RegistryKey_IsPerfDataKey_mD390E920EE9A798C7822D0E3DF1D149FB7CDB3B3(__this, NULL);
		if (!L_4)
		{
			goto IL_003c;
		}
	}
	{
		RegistryKey_ClosePerfDataKey_mA9A7893C2D9C587AD7F750CB3AF290BDAF57EE39(__this, NULL);
	}

IL_003c:
	{
		return;
	}
}
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR RegistryKey_t8752E3D024B69A6A9CC30AEB0E75F590C74E30D4* RegistryKey_OpenBaseKey_m894C0EE066C153D68657AF07BD3D5C6B4D53FA0D (int32_t ___0_hKey, int32_t ___1_view, const RuntimeMethod* method) 
{
	static bool s_Il2CppMethodInitialized;
	if (!s_Il2CppMethodInitialized)
	{
		il2cpp_codegen_initialize_runtime_metadata((uintptr_t*)&RegistryKey_t8752E3D024B69A6A9CC30AEB0E75F590C74E30D4_il2cpp_TypeInfo_var);
		s_Il2CppMethodInitialized = true;
	}
	{
		int32_t L_0 = ___1_view;
		il2cpp_codegen_runtime_class_init_inline(RegistryKey_t8752E3D024B69A6A9CC30AEB0E75F590C74E30D4_il2cpp_TypeInfo_var);
		RegistryKey_ValidateKeyView_m6750A1F58ACF19E810049D095CCF0744807879F0(L_0, NULL);
		int32_t L_1 = ___0_hKey;
		int32_t L_2 = ___1_view;
		RegistryKey_t8752E3D024B69A6A9CC30AEB0E75F590C74E30D4* L_3;
		L_3 = RegistryKey_OpenBaseKeyCore_mC9589E517BD09EBA2BAAFE07DEFA4D5D7D992151(L_1, L_2, NULL);
		return L_3;
	}
}
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR RegistryKey_t8752E3D024B69A6A9CC30AEB0E75F590C74E30D4* RegistryKey_OpenSubKey_m426543FED1DFBA420BF3D5B0A792800B53878CDA (RegistryKey_t8752E3D024B69A6A9CC30AEB0E75F590C74E30D4* __this, String_t* ___0_name, const RuntimeMethod* method) 
{
	{
		String_t* L_0 = ___0_name;
		RegistryKey_t8752E3D024B69A6A9CC30AEB0E75F590C74E30D4* L_1;
		L_1 = RegistryKey_OpenSubKey_mD3011C61B6D1D74DDB0588A2474C8A169EC52F7F(__this, L_0, (bool)0, NULL);
		return L_1;
	}
}
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR RegistryKey_t8752E3D024B69A6A9CC30AEB0E75F590C74E30D4* RegistryKey_OpenSubKey_mD3011C61B6D1D74DDB0588A2474C8A169EC52F7F (RegistryKey_t8752E3D024B69A6A9CC30AEB0E75F590C74E30D4* __this, String_t* ___0_name, bool ___1_writable, const RuntimeMethod* method) 
{
	static bool s_Il2CppMethodInitialized;
	if (!s_Il2CppMethodInitialized)
	{
		il2cpp_codegen_initialize_runtime_metadata((uintptr_t*)&RegistryKey_t8752E3D024B69A6A9CC30AEB0E75F590C74E30D4_il2cpp_TypeInfo_var);
		s_Il2CppMethodInitialized = true;
	}
	{
		String_t* L_0 = ___0_name;
		il2cpp_codegen_runtime_class_init_inline(RegistryKey_t8752E3D024B69A6A9CC30AEB0E75F590C74E30D4_il2cpp_TypeInfo_var);
		RegistryKey_ValidateKeyName_m738969C019B38EEA5B8C3C25BB7D4576FE0EE203(L_0, NULL);
		RegistryKey_EnsureNotDisposed_m8BE19DC7F1E7B9C2123D3BB6416905EFE68AFD78(__this, NULL);
		String_t* L_1 = ___0_name;
		String_t* L_2;
		L_2 = RegistryKey_FixupName_mEEBBCFD547C00AAEAAA3D54711D12C65C9B4078F(L_1, NULL);
		___0_name = L_2;
		String_t* L_3 = ___0_name;
		bool L_4 = ___1_writable;
		RegistryKey_t8752E3D024B69A6A9CC30AEB0E75F590C74E30D4* L_5;
		L_5 = RegistryKey_InternalOpenSubKeyCore_mD0430381A6C05276BF333D5FF370C3CED944BFB3(__this, L_3, L_4, NULL);
		return L_5;
	}
}
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR int32_t RegistryKey_get_SubKeyCount_mAE3905F230631A9AA8BD138BCF3325F237AD7F24 (RegistryKey_t8752E3D024B69A6A9CC30AEB0E75F590C74E30D4* __this, const RuntimeMethod* method) 
{
	{
		RegistryKey_EnsureNotDisposed_m8BE19DC7F1E7B9C2123D3BB6416905EFE68AFD78(__this, NULL);
		int32_t L_0;
		L_0 = RegistryKey_InternalSubKeyCountCore_mC707C8439FDE18743C1ADB336C72E30C83B61995(__this, NULL);
		return L_0;
	}
}
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR StringU5BU5D_t7674CD946EC0CE7B3AE0BE70E6EE85F2ECD9F248* RegistryKey_GetSubKeyNames_m10D045EFDB5670B6E5AC57850964A31A1EA01B38 (RegistryKey_t8752E3D024B69A6A9CC30AEB0E75F590C74E30D4* __this, const RuntimeMethod* method) 
{
	static bool s_Il2CppMethodInitialized;
	if (!s_Il2CppMethodInitialized)
	{
		il2cpp_codegen_initialize_runtime_metadata((uintptr_t*)&Array_Empty_TisString_t_m9832B70DF2B936246FE60F75D3D12CB946C39D16_RuntimeMethod_var);
		s_Il2CppMethodInitialized = true;
	}
	int32_t V_0 = 0;
	{
		RegistryKey_EnsureNotDisposed_m8BE19DC7F1E7B9C2123D3BB6416905EFE68AFD78(__this, NULL);
		int32_t L_0;
		L_0 = RegistryKey_get_SubKeyCount_mAE3905F230631A9AA8BD138BCF3325F237AD7F24(__this, NULL);
		V_0 = L_0;
		int32_t L_1 = V_0;
		if ((((int32_t)L_1) > ((int32_t)0)))
		{
			goto IL_0017;
		}
	}
	{
		StringU5BU5D_t7674CD946EC0CE7B3AE0BE70E6EE85F2ECD9F248* L_2;
		L_2 = Array_Empty_TisString_t_m9832B70DF2B936246FE60F75D3D12CB946C39D16_inline(Array_Empty_TisString_t_m9832B70DF2B936246FE60F75D3D12CB946C39D16_RuntimeMethod_var);
		return L_2;
	}

IL_0017:
	{
		int32_t L_3 = V_0;
		StringU5BU5D_t7674CD946EC0CE7B3AE0BE70E6EE85F2ECD9F248* L_4;
		L_4 = RegistryKey_InternalGetSubKeyNamesCore_m715FD9D02D73A1BE3FA48509AB2378A1880A798A(__this, L_3, NULL);
		return L_4;
	}
}
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR RuntimeObject* RegistryKey_GetValue_mF0F6279CFA567EBD0B939A597BABB77EB28DA94D (RegistryKey_t8752E3D024B69A6A9CC30AEB0E75F590C74E30D4* __this, String_t* ___0_name, const RuntimeMethod* method) 
{
	{
		String_t* L_0 = ___0_name;
		RuntimeObject* L_1;
		L_1 = RegistryKey_InternalGetValue_m37B31B8390738F2E98F6138DD12FB86CEECC1ADB(__this, L_0, NULL, (bool)0, NULL);
		return L_1;
	}
}
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR RuntimeObject* RegistryKey_InternalGetValue_m37B31B8390738F2E98F6138DD12FB86CEECC1ADB (RegistryKey_t8752E3D024B69A6A9CC30AEB0E75F590C74E30D4* __this, String_t* ___0_name, RuntimeObject* ___1_defaultValue, bool ___2_doNotExpand, const RuntimeMethod* method) 
{
	{
		RegistryKey_EnsureNotDisposed_m8BE19DC7F1E7B9C2123D3BB6416905EFE68AFD78(__this, NULL);
		String_t* L_0 = ___0_name;
		RuntimeObject* L_1 = ___1_defaultValue;
		bool L_2 = ___2_doNotExpand;
		RuntimeObject* L_3;
		L_3 = RegistryKey_InternalGetValueCore_m139A3171C748AAD22E972CB0048FD87655B00ACA(__this, L_0, L_1, L_2, NULL);
		return L_3;
	}
}
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR String_t* RegistryKey_FixupName_mEEBBCFD547C00AAEAAA3D54711D12C65C9B4078F (String_t* ___0_name, const RuntimeMethod* method) 
{
	static bool s_Il2CppMethodInitialized;
	if (!s_Il2CppMethodInitialized)
	{
		il2cpp_codegen_initialize_runtime_metadata((uintptr_t*)&RegistryKey_t8752E3D024B69A6A9CC30AEB0E75F590C74E30D4_il2cpp_TypeInfo_var);
		il2cpp_codegen_initialize_runtime_metadata((uintptr_t*)&StringBuilder_t_il2cpp_TypeInfo_var);
		s_Il2CppMethodInitialized = true;
	}
	StringBuilder_t* V_0 = NULL;
	int32_t V_1 = 0;
	{
		String_t* L_0 = ___0_name;
		NullCheck(L_0);
		int32_t L_1;
		L_1 = String_IndexOf_mE21E78F35EF4A7768E385A72814C88D22B689966(L_0, ((int32_t)92), NULL);
		if ((!(((uint32_t)L_1) == ((uint32_t)(-1)))))
		{
			goto IL_000d;
		}
	}
	{
		String_t* L_2 = ___0_name;
		return L_2;
	}

IL_000d:
	{
		String_t* L_3 = ___0_name;
		StringBuilder_t* L_4 = (StringBuilder_t*)il2cpp_codegen_object_new(StringBuilder_t_il2cpp_TypeInfo_var);
		StringBuilder__ctor_mCD797D942316CB356205FD96415B0B7581CDAD60(L_4, L_3, NULL);
		V_0 = L_4;
		StringBuilder_t* L_5 = V_0;
		il2cpp_codegen_runtime_class_init_inline(RegistryKey_t8752E3D024B69A6A9CC30AEB0E75F590C74E30D4_il2cpp_TypeInfo_var);
		RegistryKey_FixupPath_m565CE6B665A2A4371F135478D9B19CC22806691A(L_5, NULL);
		StringBuilder_t* L_6 = V_0;
		NullCheck(L_6);
		int32_t L_7;
		L_7 = StringBuilder_get_Length_mDEA041E7357C68CC3B5885276BB403676DAAE0D8(L_6, NULL);
		V_1 = ((int32_t)il2cpp_codegen_subtract(L_7, 1));
		int32_t L_8 = V_1;
		if ((((int32_t)L_8) < ((int32_t)0)))
		{
			goto IL_0039;
		}
	}
	{
		StringBuilder_t* L_9 = V_0;
		int32_t L_10 = V_1;
		NullCheck(L_9);
		Il2CppChar L_11;
		L_11 = StringBuilder_get_Chars_m254FD6F2F75C00B0D353D73B2A4A19316BD7624D(L_9, L_10, NULL);
		if ((!(((uint32_t)L_11) == ((uint32_t)((int32_t)92)))))
		{
			goto IL_0039;
		}
	}
	{
		StringBuilder_t* L_12 = V_0;
		int32_t L_13 = V_1;
		NullCheck(L_12);
		StringBuilder_set_Length_mE2427BDAEF91C4E4A6C80F3BDF1F6E01DBCC2414(L_12, L_13, NULL);
	}

IL_0039:
	{
		StringBuilder_t* L_14 = V_0;
		NullCheck(L_14);
		String_t* L_15;
		L_15 = VirtualFuncInvoker0< String_t* >::Invoke(3, L_14);
		return L_15;
	}
}
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR void RegistryKey_FixupPath_m565CE6B665A2A4371F135478D9B19CC22806691A (StringBuilder_t* ___0_path, const RuntimeMethod* method) 
{
	int32_t V_0 = 0;
	bool V_1 = false;
	Il2CppChar V_2 = 0x0;
	int32_t V_3 = 0;
	int32_t V_4 = 0;
	{
		StringBuilder_t* L_0 = ___0_path;
		NullCheck(L_0);
		int32_t L_1;
		L_1 = StringBuilder_get_Length_mDEA041E7357C68CC3B5885276BB403676DAAE0D8(L_0, NULL);
		V_0 = L_1;
		V_1 = (bool)0;
		V_2 = ((int32_t)65535);
		V_3 = 1;
		goto IL_0045;
	}

IL_0013:
	{
		StringBuilder_t* L_2 = ___0_path;
		int32_t L_3 = V_3;
		NullCheck(L_2);
		Il2CppChar L_4;
		L_4 = StringBuilder_get_Chars_m254FD6F2F75C00B0D353D73B2A4A19316BD7624D(L_2, L_3, NULL);
		if ((!(((uint32_t)L_4) == ((uint32_t)((int32_t)92)))))
		{
			goto IL_0041;
		}
	}
	{
		int32_t L_5 = V_3;
		V_3 = ((int32_t)il2cpp_codegen_add(L_5, 1));
		goto IL_0032;
	}

IL_0024:
	{
		StringBuilder_t* L_6 = ___0_path;
		int32_t L_7 = V_3;
		Il2CppChar L_8 = V_2;
		NullCheck(L_6);
		StringBuilder_set_Chars_m20B53B0EEAB2A0BB0EC84A130FF12EA86ADD99AE(L_6, L_7, L_8, NULL);
		int32_t L_9 = V_3;
		V_3 = ((int32_t)il2cpp_codegen_add(L_9, 1));
		V_1 = (bool)1;
	}

IL_0032:
	{
		int32_t L_10 = V_3;
		int32_t L_11 = V_0;
		if ((((int32_t)L_10) >= ((int32_t)L_11)))
		{
			goto IL_0041;
		}
	}
	{
		StringBuilder_t* L_12 = ___0_path;
		int32_t L_13 = V_3;
		NullCheck(L_12);
		Il2CppChar L_14;
		L_14 = StringBuilder_get_Chars_m254FD6F2F75C00B0D353D73B2A4A19316BD7624D(L_12, L_13, NULL);
		if ((((int32_t)L_14) == ((int32_t)((int32_t)92))))
		{
			goto IL_0024;
		}
	}

IL_0041:
	{
		int32_t L_15 = V_3;
		V_3 = ((int32_t)il2cpp_codegen_add(L_15, 1));
	}

IL_0045:
	{
		int32_t L_16 = V_3;
		int32_t L_17 = V_0;
		if ((((int32_t)L_16) < ((int32_t)((int32_t)il2cpp_codegen_subtract(L_17, 1)))))
		{
			goto IL_0013;
		}
	}
	{
		bool L_18 = V_1;
		if (!L_18)
		{
			goto IL_0093;
		}
	}
	{
		V_3 = 0;
		V_4 = 0;
		goto IL_007e;
	}

IL_0055:
	{
		StringBuilder_t* L_19 = ___0_path;
		int32_t L_20 = V_3;
		NullCheck(L_19);
		Il2CppChar L_21;
		L_21 = StringBuilder_get_Chars_m254FD6F2F75C00B0D353D73B2A4A19316BD7624D(L_19, L_20, NULL);
		Il2CppChar L_22 = V_2;
		if ((!(((uint32_t)L_21) == ((uint32_t)L_22))))
		{
			goto IL_0065;
		}
	}
	{
		int32_t L_23 = V_3;
		V_3 = ((int32_t)il2cpp_codegen_add(L_23, 1));
		goto IL_007e;
	}

IL_0065:
	{
		StringBuilder_t* L_24 = ___0_path;
		int32_t L_25 = V_4;
		StringBuilder_t* L_26 = ___0_path;
		int32_t L_27 = V_3;
		NullCheck(L_26);
		Il2CppChar L_28;
		L_28 = StringBuilder_get_Chars_m254FD6F2F75C00B0D353D73B2A4A19316BD7624D(L_26, L_27, NULL);
		NullCheck(L_24);
		StringBuilder_set_Chars_m20B53B0EEAB2A0BB0EC84A130FF12EA86ADD99AE(L_24, L_25, L_28, NULL);
		int32_t L_29 = V_3;
		V_3 = ((int32_t)il2cpp_codegen_add(L_29, 1));
		int32_t L_30 = V_4;
		V_4 = ((int32_t)il2cpp_codegen_add(L_30, 1));
	}

IL_007e:
	{
		int32_t L_31 = V_3;
		int32_t L_32 = V_0;
		if ((((int32_t)L_31) < ((int32_t)L_32)))
		{
			goto IL_0055;
		}
	}
	{
		StringBuilder_t* L_33 = ___0_path;
		StringBuilder_t* L_34 = L_33;
		NullCheck(L_34);
		int32_t L_35;
		L_35 = StringBuilder_get_Length_mDEA041E7357C68CC3B5885276BB403676DAAE0D8(L_34, NULL);
		int32_t L_36 = V_4;
		int32_t L_37 = V_3;
		NullCheck(L_34);
		StringBuilder_set_Length_mE2427BDAEF91C4E4A6C80F3BDF1F6E01DBCC2414(L_34, ((int32_t)il2cpp_codegen_add(L_35, ((int32_t)il2cpp_codegen_subtract(L_36, L_37)))), NULL);
	}

IL_0093:
	{
		return;
	}
}
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR void RegistryKey_EnsureNotDisposed_m8BE19DC7F1E7B9C2123D3BB6416905EFE68AFD78 (RegistryKey_t8752E3D024B69A6A9CC30AEB0E75F590C74E30D4* __this, const RuntimeMethod* method) 
{
	{
		SafeRegistryHandle_t890BD43C81043709A2103F1FDC4394C603EE1FDF* L_0 = __this->____hkey;
		il2cpp_codegen_memory_barrier();
		if (L_0)
		{
			goto IL_001d;
		}
	}
	{
		String_t* L_1 = __this->____keyName;
		il2cpp_codegen_memory_barrier();
		il2cpp_codegen_runtime_class_init_inline(((RuntimeClass*)il2cpp_codegen_initialize_runtime_metadata_inline((uintptr_t*)&SR_tCF5D02DC363D3707E4B0700773B397B107D749CF_il2cpp_TypeInfo_var)));
		String_t* L_2;
		L_2 = SR_get_ObjectDisposed_RegKeyClosed_mFB336460D1656E0787CC0C825DEEAA98AD2F59F6(NULL);
		ObjectDisposedException_tC5FB29E8E980E2010A2F6A5B9B791089419F89EB* L_3 = (ObjectDisposedException_tC5FB29E8E980E2010A2F6A5B9B791089419F89EB*)il2cpp_codegen_object_new(((RuntimeClass*)il2cpp_codegen_initialize_runtime_metadata_inline((uintptr_t*)&ObjectDisposedException_tC5FB29E8E980E2010A2F6A5B9B791089419F89EB_il2cpp_TypeInfo_var)));
		ObjectDisposedException__ctor_m5C356C25295E89559C120CB4562783AAF7F41C84(L_3, L_1, L_2, NULL);
		IL2CPP_RAISE_MANAGED_EXCEPTION(L_3, ((RuntimeMethod*)il2cpp_codegen_initialize_runtime_metadata_inline((uintptr_t*)&RegistryKey_EnsureNotDisposed_m8BE19DC7F1E7B9C2123D3BB6416905EFE68AFD78_RuntimeMethod_var)));
	}

IL_001d:
	{
		return;
	}
}
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR void RegistryKey_ValidateKeyName_m738969C019B38EEA5B8C3C25BB7D4576FE0EE203 (String_t* ___0_name, const RuntimeMethod* method) 
{
	static bool s_Il2CppMethodInitialized;
	if (!s_Il2CppMethodInitialized)
	{
		il2cpp_codegen_initialize_runtime_metadata((uintptr_t*)&_stringLiteral09B11B6CC411D8B9FFB75EAAE9A35B2AF248CE40);
		s_Il2CppMethodInitialized = true;
	}
	int32_t V_0 = 0;
	int32_t V_1 = 0;
	{
		String_t* L_0 = ___0_name;
		if (L_0)
		{
			goto IL_000e;
		}
	}
	{
		ArgumentNullException_t327031E412FAB2351B0022DD5DAD47E67E597129* L_1 = (ArgumentNullException_t327031E412FAB2351B0022DD5DAD47E67E597129*)il2cpp_codegen_object_new(((RuntimeClass*)il2cpp_codegen_initialize_runtime_metadata_inline((uintptr_t*)&ArgumentNullException_t327031E412FAB2351B0022DD5DAD47E67E597129_il2cpp_TypeInfo_var)));
		ArgumentNullException__ctor_m444AE141157E333844FC1A9500224C2F9FD24F4B(L_1, ((String_t*)il2cpp_codegen_initialize_runtime_metadata_inline((uintptr_t*)&_stringLiteralCE18B047107AA23D1AA9B2ED32D316148E02655F)), NULL);
		IL2CPP_RAISE_MANAGED_EXCEPTION(L_1, ((RuntimeMethod*)il2cpp_codegen_initialize_runtime_metadata_inline((uintptr_t*)&RegistryKey_ValidateKeyName_m738969C019B38EEA5B8C3C25BB7D4576FE0EE203_RuntimeMethod_var)));
	}

IL_000e:
	{
		String_t* L_2 = ___0_name;
		NullCheck(L_2);
		int32_t L_3;
		L_3 = String_IndexOf_m490810CB7ADA9230AC0F8D78E213A8EFED129F55(L_2, _stringLiteral09B11B6CC411D8B9FFB75EAAE9A35B2AF248CE40, 5, NULL);
		V_0 = L_3;
		V_1 = 0;
		goto IL_004b;
	}

IL_001f:
	{
		int32_t L_4 = V_0;
		int32_t L_5 = V_1;
		if ((((int32_t)((int32_t)il2cpp_codegen_subtract(L_4, L_5))) <= ((int32_t)((int32_t)255))))
		{
			goto IL_0039;
		}
	}
	{
		il2cpp_codegen_runtime_class_init_inline(((RuntimeClass*)il2cpp_codegen_initialize_runtime_metadata_inline((uintptr_t*)&SR_tCF5D02DC363D3707E4B0700773B397B107D749CF_il2cpp_TypeInfo_var)));
		String_t* L_6;
		L_6 = SR_get_Arg_RegKeyStrLenBug_m3A198859EF9C55F4D81EC9E1A9985531D8F0C44F(NULL);
		ArgumentException_tAD90411542A20A9C72D5CDA3A84181D8B947A263* L_7 = (ArgumentException_tAD90411542A20A9C72D5CDA3A84181D8B947A263*)il2cpp_codegen_object_new(((RuntimeClass*)il2cpp_codegen_initialize_runtime_metadata_inline((uintptr_t*)&ArgumentException_tAD90411542A20A9C72D5CDA3A84181D8B947A263_il2cpp_TypeInfo_var)));
		ArgumentException__ctor_m8F9D40CE19D19B698A70F9A258640EB52DB39B62(L_7, L_6, ((String_t*)il2cpp_codegen_initialize_runtime_metadata_inline((uintptr_t*)&_stringLiteralCE18B047107AA23D1AA9B2ED32D316148E02655F)), NULL);
		IL2CPP_RAISE_MANAGED_EXCEPTION(L_7, ((RuntimeMethod*)il2cpp_codegen_initialize_runtime_metadata_inline((uintptr_t*)&RegistryKey_ValidateKeyName_m738969C019B38EEA5B8C3C25BB7D4576FE0EE203_RuntimeMethod_var)));
	}

IL_0039:
	{
		int32_t L_8 = V_0;
		V_1 = ((int32_t)il2cpp_codegen_add(L_8, 1));
		String_t* L_9 = ___0_name;
		int32_t L_10 = V_1;
		NullCheck(L_9);
		int32_t L_11;
		L_11 = String_IndexOf_m2DFDE7BD37585BDBCD6F688B4E4A93304235A0B8(L_9, _stringLiteral09B11B6CC411D8B9FFB75EAAE9A35B2AF248CE40, L_10, 5, NULL);
		V_0 = L_11;
	}

IL_004b:
	{
		int32_t L_12 = V_0;
		if ((!(((uint32_t)L_12) == ((uint32_t)(-1)))))
		{
			goto IL_001f;
		}
	}
	{
		String_t* L_13 = ___0_name;
		NullCheck(L_13);
		int32_t L_14;
		L_14 = String_get_Length_m42625D67623FA5CC7A44D47425CE86FB946542D2_inline(L_13, NULL);
		int32_t L_15 = V_1;
		if ((((int32_t)((int32_t)il2cpp_codegen_subtract(L_14, L_15))) <= ((int32_t)((int32_t)255))))
		{
			goto IL_006e;
		}
	}
	{
		il2cpp_codegen_runtime_class_init_inline(((RuntimeClass*)il2cpp_codegen_initialize_runtime_metadata_inline((uintptr_t*)&SR_tCF5D02DC363D3707E4B0700773B397B107D749CF_il2cpp_TypeInfo_var)));
		String_t* L_16;
		L_16 = SR_get_Arg_RegKeyStrLenBug_m3A198859EF9C55F4D81EC9E1A9985531D8F0C44F(NULL);
		ArgumentException_tAD90411542A20A9C72D5CDA3A84181D8B947A263* L_17 = (ArgumentException_tAD90411542A20A9C72D5CDA3A84181D8B947A263*)il2cpp_codegen_object_new(((RuntimeClass*)il2cpp_codegen_initialize_runtime_metadata_inline((uintptr_t*)&ArgumentException_tAD90411542A20A9C72D5CDA3A84181D8B947A263_il2cpp_TypeInfo_var)));
		ArgumentException__ctor_m8F9D40CE19D19B698A70F9A258640EB52DB39B62(L_17, L_16, ((String_t*)il2cpp_codegen_initialize_runtime_metadata_inline((uintptr_t*)&_stringLiteralCE18B047107AA23D1AA9B2ED32D316148E02655F)), NULL);
		IL2CPP_RAISE_MANAGED_EXCEPTION(L_17, ((RuntimeMethod*)il2cpp_codegen_initialize_runtime_metadata_inline((uintptr_t*)&RegistryKey_ValidateKeyName_m738969C019B38EEA5B8C3C25BB7D4576FE0EE203_RuntimeMethod_var)));
	}

IL_006e:
	{
		return;
	}
}
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR void RegistryKey_ValidateKeyView_m6750A1F58ACF19E810049D095CCF0744807879F0 (int32_t ___0_view, const RuntimeMethod* method) 
{
	{
		int32_t L_0 = ___0_view;
		if (!L_0)
		{
			goto IL_0023;
		}
	}
	{
		int32_t L_1 = ___0_view;
		if ((((int32_t)L_1) == ((int32_t)((int32_t)512))))
		{
			goto IL_0023;
		}
	}
	{
		int32_t L_2 = ___0_view;
		if ((((int32_t)L_2) == ((int32_t)((int32_t)256))))
		{
			goto IL_0023;
		}
	}
	{
		il2cpp_codegen_runtime_class_init_inline(((RuntimeClass*)il2cpp_codegen_initialize_runtime_metadata_inline((uintptr_t*)&SR_tCF5D02DC363D3707E4B0700773B397B107D749CF_il2cpp_TypeInfo_var)));
		String_t* L_3;
		L_3 = SR_get_Argument_InvalidRegistryViewCheck_m0CBA72BB49ACC26AB5AFF62EF8E1874C9EABD2CF(NULL);
		ArgumentException_tAD90411542A20A9C72D5CDA3A84181D8B947A263* L_4 = (ArgumentException_tAD90411542A20A9C72D5CDA3A84181D8B947A263*)il2cpp_codegen_object_new(((RuntimeClass*)il2cpp_codegen_initialize_runtime_metadata_inline((uintptr_t*)&ArgumentException_tAD90411542A20A9C72D5CDA3A84181D8B947A263_il2cpp_TypeInfo_var)));
		ArgumentException__ctor_m8F9D40CE19D19B698A70F9A258640EB52DB39B62(L_4, L_3, ((String_t*)il2cpp_codegen_initialize_runtime_metadata_inline((uintptr_t*)&_stringLiteral1D987D14D8E0C888F1095B9A3F3E261A95CEACCC)), NULL);
		IL2CPP_RAISE_MANAGED_EXCEPTION(L_4, ((RuntimeMethod*)il2cpp_codegen_initialize_runtime_metadata_inline((uintptr_t*)&RegistryKey_ValidateKeyView_m6750A1F58ACF19E810049D095CCF0744807879F0_RuntimeMethod_var)));
	}

IL_0023:
	{
		return;
	}
}
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR bool RegistryKey_IsSystemKey_m9E0980A65B2FBD73C34EC940D7572F90AF33D7B6 (RegistryKey_t8752E3D024B69A6A9CC30AEB0E75F590C74E30D4* __this, const RuntimeMethod* method) 
{
	{
		int32_t L_0 = __this->____state;
		il2cpp_codegen_memory_barrier();
		return (bool)((!(((uint32_t)((int32_t)((int32_t)L_0&2))) <= ((uint32_t)0)))? 1 : 0);
	}
}
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR bool RegistryKey_IsPerfDataKey_mD390E920EE9A798C7822D0E3DF1D149FB7CDB3B3 (RegistryKey_t8752E3D024B69A6A9CC30AEB0E75F590C74E30D4* __this, const RuntimeMethod* method) 
{
	{
		int32_t L_0 = __this->____state;
		il2cpp_codegen_memory_barrier();
		return (bool)((!(((uint32_t)((int32_t)((int32_t)L_0&8))) <= ((uint32_t)0)))? 1 : 0);
	}
}
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR void RegistryKey_ClosePerfDataKey_mA9A7893C2D9C587AD7F750CB3AF290BDAF57EE39 (RegistryKey_t8752E3D024B69A6A9CC30AEB0E75F590C74E30D4* __this, const RuntimeMethod* method) 
{
	{
		il2cpp_codegen_runtime_class_init_inline(((RuntimeClass*)il2cpp_codegen_initialize_runtime_metadata_inline((uintptr_t*)&SR_tCF5D02DC363D3707E4B0700773B397B107D749CF_il2cpp_TypeInfo_var)));
		String_t* L_0;
		L_0 = SR_get_PlatformNotSupported_Registry_m5C3864E998C39F3B499E6155FC86D40B96A13B68(NULL);
		PlatformNotSupportedException_tD2BD7EB9278518AA5FE8AE75AD5D0D4298A4631A* L_1 = (PlatformNotSupportedException_tD2BD7EB9278518AA5FE8AE75AD5D0D4298A4631A*)il2cpp_codegen_object_new(((RuntimeClass*)il2cpp_codegen_initialize_runtime_metadata_inline((uintptr_t*)&PlatformNotSupportedException_tD2BD7EB9278518AA5FE8AE75AD5D0D4298A4631A_il2cpp_TypeInfo_var)));
		PlatformNotSupportedException__ctor_mC5103EE3FE4FE245039B1107D6685296D9CC6560(L_1, L_0, NULL);
		IL2CPP_RAISE_MANAGED_EXCEPTION(L_1, ((RuntimeMethod*)il2cpp_codegen_initialize_runtime_metadata_inline((uintptr_t*)&RegistryKey_ClosePerfDataKey_mA9A7893C2D9C587AD7F750CB3AF290BDAF57EE39_RuntimeMethod_var)));
	}
}
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR RegistryKey_t8752E3D024B69A6A9CC30AEB0E75F590C74E30D4* RegistryKey_OpenBaseKeyCore_mC9589E517BD09EBA2BAAFE07DEFA4D5D7D992151 (int32_t ___0_hKey, int32_t ___1_view, const RuntimeMethod* method) 
{
	{
		il2cpp_codegen_runtime_class_init_inline(((RuntimeClass*)il2cpp_codegen_initialize_runtime_metadata_inline((uintptr_t*)&SR_tCF5D02DC363D3707E4B0700773B397B107D749CF_il2cpp_TypeInfo_var)));
		String_t* L_0;
		L_0 = SR_get_PlatformNotSupported_Registry_m5C3864E998C39F3B499E6155FC86D40B96A13B68(NULL);
		PlatformNotSupportedException_tD2BD7EB9278518AA5FE8AE75AD5D0D4298A4631A* L_1 = (PlatformNotSupportedException_tD2BD7EB9278518AA5FE8AE75AD5D0D4298A4631A*)il2cpp_codegen_object_new(((RuntimeClass*)il2cpp_codegen_initialize_runtime_metadata_inline((uintptr_t*)&PlatformNotSupportedException_tD2BD7EB9278518AA5FE8AE75AD5D0D4298A4631A_il2cpp_TypeInfo_var)));
		PlatformNotSupportedException__ctor_mC5103EE3FE4FE245039B1107D6685296D9CC6560(L_1, L_0, NULL);
		IL2CPP_RAISE_MANAGED_EXCEPTION(L_1, ((RuntimeMethod*)il2cpp_codegen_initialize_runtime_metadata_inline((uintptr_t*)&RegistryKey_OpenBaseKeyCore_mC9589E517BD09EBA2BAAFE07DEFA4D5D7D992151_RuntimeMethod_var)));
	}
}
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR RegistryKey_t8752E3D024B69A6A9CC30AEB0E75F590C74E30D4* RegistryKey_InternalOpenSubKeyCore_mD0430381A6C05276BF333D5FF370C3CED944BFB3 (RegistryKey_t8752E3D024B69A6A9CC30AEB0E75F590C74E30D4* __this, String_t* ___0_name, bool ___1_writable, const RuntimeMethod* method) 
{
	{
		il2cpp_codegen_runtime_class_init_inline(((RuntimeClass*)il2cpp_codegen_initialize_runtime_metadata_inline((uintptr_t*)&SR_tCF5D02DC363D3707E4B0700773B397B107D749CF_il2cpp_TypeInfo_var)));
		String_t* L_0;
		L_0 = SR_get_PlatformNotSupported_Registry_m5C3864E998C39F3B499E6155FC86D40B96A13B68(NULL);
		PlatformNotSupportedException_tD2BD7EB9278518AA5FE8AE75AD5D0D4298A4631A* L_1 = (PlatformNotSupportedException_tD2BD7EB9278518AA5FE8AE75AD5D0D4298A4631A*)il2cpp_codegen_object_new(((RuntimeClass*)il2cpp_codegen_initialize_runtime_metadata_inline((uintptr_t*)&PlatformNotSupportedException_tD2BD7EB9278518AA5FE8AE75AD5D0D4298A4631A_il2cpp_TypeInfo_var)));
		PlatformNotSupportedException__ctor_mC5103EE3FE4FE245039B1107D6685296D9CC6560(L_1, L_0, NULL);
		IL2CPP_RAISE_MANAGED_EXCEPTION(L_1, ((RuntimeMethod*)il2cpp_codegen_initialize_runtime_metadata_inline((uintptr_t*)&RegistryKey_InternalOpenSubKeyCore_mD0430381A6C05276BF333D5FF370C3CED944BFB3_RuntimeMethod_var)));
	}
}
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR int32_t RegistryKey_InternalSubKeyCountCore_mC707C8439FDE18743C1ADB336C72E30C83B61995 (RegistryKey_t8752E3D024B69A6A9CC30AEB0E75F590C74E30D4* __this, const RuntimeMethod* method) 
{
	{
		il2cpp_codegen_runtime_class_init_inline(((RuntimeClass*)il2cpp_codegen_initialize_runtime_metadata_inline((uintptr_t*)&SR_tCF5D02DC363D3707E4B0700773B397B107D749CF_il2cpp_TypeInfo_var)));
		String_t* L_0;
		L_0 = SR_get_PlatformNotSupported_Registry_m5C3864E998C39F3B499E6155FC86D40B96A13B68(NULL);
		PlatformNotSupportedException_tD2BD7EB9278518AA5FE8AE75AD5D0D4298A4631A* L_1 = (PlatformNotSupportedException_tD2BD7EB9278518AA5FE8AE75AD5D0D4298A4631A*)il2cpp_codegen_object_new(((RuntimeClass*)il2cpp_codegen_initialize_runtime_metadata_inline((uintptr_t*)&PlatformNotSupportedException_tD2BD7EB9278518AA5FE8AE75AD5D0D4298A4631A_il2cpp_TypeInfo_var)));
		PlatformNotSupportedException__ctor_mC5103EE3FE4FE245039B1107D6685296D9CC6560(L_1, L_0, NULL);
		IL2CPP_RAISE_MANAGED_EXCEPTION(L_1, ((RuntimeMethod*)il2cpp_codegen_initialize_runtime_metadata_inline((uintptr_t*)&RegistryKey_InternalSubKeyCountCore_mC707C8439FDE18743C1ADB336C72E30C83B61995_RuntimeMethod_var)));
	}
}
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR StringU5BU5D_t7674CD946EC0CE7B3AE0BE70E6EE85F2ECD9F248* RegistryKey_InternalGetSubKeyNamesCore_m715FD9D02D73A1BE3FA48509AB2378A1880A798A (RegistryKey_t8752E3D024B69A6A9CC30AEB0E75F590C74E30D4* __this, int32_t ___0_subkeys, const RuntimeMethod* method) 
{
	{
		il2cpp_codegen_runtime_class_init_inline(((RuntimeClass*)il2cpp_codegen_initialize_runtime_metadata_inline((uintptr_t*)&SR_tCF5D02DC363D3707E4B0700773B397B107D749CF_il2cpp_TypeInfo_var)));
		String_t* L_0;
		L_0 = SR_get_PlatformNotSupported_Registry_m5C3864E998C39F3B499E6155FC86D40B96A13B68(NULL);
		PlatformNotSupportedException_tD2BD7EB9278518AA5FE8AE75AD5D0D4298A4631A* L_1 = (PlatformNotSupportedException_tD2BD7EB9278518AA5FE8AE75AD5D0D4298A4631A*)il2cpp_codegen_object_new(((RuntimeClass*)il2cpp_codegen_initialize_runtime_metadata_inline((uintptr_t*)&PlatformNotSupportedException_tD2BD7EB9278518AA5FE8AE75AD5D0D4298A4631A_il2cpp_TypeInfo_var)));
		PlatformNotSupportedException__ctor_mC5103EE3FE4FE245039B1107D6685296D9CC6560(L_1, L_0, NULL);
		IL2CPP_RAISE_MANAGED_EXCEPTION(L_1, ((RuntimeMethod*)il2cpp_codegen_initialize_runtime_metadata_inline((uintptr_t*)&RegistryKey_InternalGetSubKeyNamesCore_m715FD9D02D73A1BE3FA48509AB2378A1880A798A_RuntimeMethod_var)));
	}
}
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR RuntimeObject* RegistryKey_InternalGetValueCore_m139A3171C748AAD22E972CB0048FD87655B00ACA (RegistryKey_t8752E3D024B69A6A9CC30AEB0E75F590C74E30D4* __this, String_t* ___0_name, RuntimeObject* ___1_defaultValue, bool ___2_doNotExpand, const RuntimeMethod* method) 
{
	{
		il2cpp_codegen_runtime_class_init_inline(((RuntimeClass*)il2cpp_codegen_initialize_runtime_metadata_inline((uintptr_t*)&SR_tCF5D02DC363D3707E4B0700773B397B107D749CF_il2cpp_TypeInfo_var)));
		String_t* L_0;
		L_0 = SR_get_PlatformNotSupported_Registry_m5C3864E998C39F3B499E6155FC86D40B96A13B68(NULL);
		PlatformNotSupportedException_tD2BD7EB9278518AA5FE8AE75AD5D0D4298A4631A* L_1 = (PlatformNotSupportedException_tD2BD7EB9278518AA5FE8AE75AD5D0D4298A4631A*)il2cpp_codegen_object_new(((RuntimeClass*)il2cpp_codegen_initialize_runtime_metadata_inline((uintptr_t*)&PlatformNotSupportedException_tD2BD7EB9278518AA5FE8AE75AD5D0D4298A4631A_il2cpp_TypeInfo_var)));
		PlatformNotSupportedException__ctor_mC5103EE3FE4FE245039B1107D6685296D9CC6560(L_1, L_0, NULL);
		IL2CPP_RAISE_MANAGED_EXCEPTION(L_1, ((RuntimeMethod*)il2cpp_codegen_initialize_runtime_metadata_inline((uintptr_t*)&RegistryKey_InternalGetValueCore_m139A3171C748AAD22E972CB0048FD87655B00ACA_RuntimeMethod_var)));
	}
}
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR void RegistryKey__cctor_m4730E60418B2747FEA3ECCC5144C40134A9B4F19 (const RuntimeMethod* method) 
{
	static bool s_Il2CppMethodInitialized;
	if (!s_Il2CppMethodInitialized)
	{
		il2cpp_codegen_initialize_runtime_metadata((uintptr_t*)&RegistryKey_t8752E3D024B69A6A9CC30AEB0E75F590C74E30D4_il2cpp_TypeInfo_var);
		il2cpp_codegen_initialize_runtime_metadata((uintptr_t*)&StringU5BU5D_t7674CD946EC0CE7B3AE0BE70E6EE85F2ECD9F248_il2cpp_TypeInfo_var);
		il2cpp_codegen_initialize_runtime_metadata((uintptr_t*)&_stringLiteral3792CAE3D944C750A90C6EAB820EBB80F23128A8);
		il2cpp_codegen_initialize_runtime_metadata((uintptr_t*)&_stringLiteral6BBACEC2AF7F52E71DDFBD94D23CB7140B770916);
		il2cpp_codegen_initialize_runtime_metadata((uintptr_t*)&_stringLiteral86A1D5983E899BFCC1B8D83C44231A4F60497E4D);
		il2cpp_codegen_initialize_runtime_metadata((uintptr_t*)&_stringLiteral8A047FD9B4CCBDFD3876EB4B4AB623EF03671DC3);
		il2cpp_codegen_initialize_runtime_metadata((uintptr_t*)&_stringLiteralDC92678F646C9D9E1B7EB843CE840E2B0420D5BF);
		il2cpp_codegen_initialize_runtime_metadata((uintptr_t*)&_stringLiteralFD48E940AB4046C2C8344BD46CB54A2ACDC31BD4);
		s_Il2CppMethodInitialized = true;
	}
	{
		intptr_t L_0;
		memset((&L_0), 0, sizeof(L_0));
		IntPtr__ctor_m20A566609A091311C734617C699E61F545250AC7((&L_0), ((int32_t)-2147483648LL), NULL);
		((RegistryKey_t8752E3D024B69A6A9CC30AEB0E75F590C74E30D4_StaticFields*)il2cpp_codegen_static_fields_for(RegistryKey_t8752E3D024B69A6A9CC30AEB0E75F590C74E30D4_il2cpp_TypeInfo_var))->___HKEY_CLASSES_ROOT = L_0;
		intptr_t L_1;
		memset((&L_1), 0, sizeof(L_1));
		IntPtr__ctor_m20A566609A091311C734617C699E61F545250AC7((&L_1), ((int32_t)-2147483647), NULL);
		((RegistryKey_t8752E3D024B69A6A9CC30AEB0E75F590C74E30D4_StaticFields*)il2cpp_codegen_static_fields_for(RegistryKey_t8752E3D024B69A6A9CC30AEB0E75F590C74E30D4_il2cpp_TypeInfo_var))->___HKEY_CURRENT_USER = L_1;
		intptr_t L_2;
		memset((&L_2), 0, sizeof(L_2));
		IntPtr__ctor_m20A566609A091311C734617C699E61F545250AC7((&L_2), ((int32_t)-2147483646), NULL);
		((RegistryKey_t8752E3D024B69A6A9CC30AEB0E75F590C74E30D4_StaticFields*)il2cpp_codegen_static_fields_for(RegistryKey_t8752E3D024B69A6A9CC30AEB0E75F590C74E30D4_il2cpp_TypeInfo_var))->___HKEY_LOCAL_MACHINE = L_2;
		intptr_t L_3;
		memset((&L_3), 0, sizeof(L_3));
		IntPtr__ctor_m20A566609A091311C734617C699E61F545250AC7((&L_3), ((int32_t)-2147483645), NULL);
		((RegistryKey_t8752E3D024B69A6A9CC30AEB0E75F590C74E30D4_StaticFields*)il2cpp_codegen_static_fields_for(RegistryKey_t8752E3D024B69A6A9CC30AEB0E75F590C74E30D4_il2cpp_TypeInfo_var))->___HKEY_USERS = L_3;
		intptr_t L_4;
		memset((&L_4), 0, sizeof(L_4));
		IntPtr__ctor_m20A566609A091311C734617C699E61F545250AC7((&L_4), ((int32_t)-2147483644), NULL);
		((RegistryKey_t8752E3D024B69A6A9CC30AEB0E75F590C74E30D4_StaticFields*)il2cpp_codegen_static_fields_for(RegistryKey_t8752E3D024B69A6A9CC30AEB0E75F590C74E30D4_il2cpp_TypeInfo_var))->___HKEY_PERFORMANCE_DATA = L_4;
		intptr_t L_5;
		memset((&L_5), 0, sizeof(L_5));
		IntPtr__ctor_m20A566609A091311C734617C699E61F545250AC7((&L_5), ((int32_t)-2147483643), NULL);
		((RegistryKey_t8752E3D024B69A6A9CC30AEB0E75F590C74E30D4_StaticFields*)il2cpp_codegen_static_fields_for(RegistryKey_t8752E3D024B69A6A9CC30AEB0E75F590C74E30D4_il2cpp_TypeInfo_var))->___HKEY_CURRENT_CONFIG = L_5;
		StringU5BU5D_t7674CD946EC0CE7B3AE0BE70E6EE85F2ECD9F248* L_6 = (StringU5BU5D_t7674CD946EC0CE7B3AE0BE70E6EE85F2ECD9F248*)(StringU5BU5D_t7674CD946EC0CE7B3AE0BE70E6EE85F2ECD9F248*)SZArrayNew(StringU5BU5D_t7674CD946EC0CE7B3AE0BE70E6EE85F2ECD9F248_il2cpp_TypeInfo_var, (uint32_t)6);
		StringU5BU5D_t7674CD946EC0CE7B3AE0BE70E6EE85F2ECD9F248* L_7 = L_6;
		NullCheck(L_7);
		(L_7)->SetAt(static_cast<il2cpp_array_size_t>(0), (String_t*)_stringLiteralDC92678F646C9D9E1B7EB843CE840E2B0420D5BF);
		StringU5BU5D_t7674CD946EC0CE7B3AE0BE70E6EE85F2ECD9F248* L_8 = L_7;
		NullCheck(L_8);
		(L_8)->SetAt(static_cast<il2cpp_array_size_t>(1), (String_t*)_stringLiteral86A1D5983E899BFCC1B8D83C44231A4F60497E4D);
		StringU5BU5D_t7674CD946EC0CE7B3AE0BE70E6EE85F2ECD9F248* L_9 = L_8;
		NullCheck(L_9);
		(L_9)->SetAt(static_cast<il2cpp_array_size_t>(2), (String_t*)_stringLiteral3792CAE3D944C750A90C6EAB820EBB80F23128A8);
		StringU5BU5D_t7674CD946EC0CE7B3AE0BE70E6EE85F2ECD9F248* L_10 = L_9;
		NullCheck(L_10);
		(L_10)->SetAt(static_cast<il2cpp_array_size_t>(3), (String_t*)_stringLiteral6BBACEC2AF7F52E71DDFBD94D23CB7140B770916);
		StringU5BU5D_t7674CD946EC0CE7B3AE0BE70E6EE85F2ECD9F248* L_11 = L_10;
		NullCheck(L_11);
		(L_11)->SetAt(static_cast<il2cpp_array_size_t>(4), (String_t*)_stringLiteral8A047FD9B4CCBDFD3876EB4B4AB623EF03671DC3);
		StringU5BU5D_t7674CD946EC0CE7B3AE0BE70E6EE85F2ECD9F248* L_12 = L_11;
		NullCheck(L_12);
		(L_12)->SetAt(static_cast<il2cpp_array_size_t>(5), (String_t*)_stringLiteralFD48E940AB4046C2C8344BD46CB54A2ACDC31BD4);
		((RegistryKey_t8752E3D024B69A6A9CC30AEB0E75F590C74E30D4_StaticFields*)il2cpp_codegen_static_fields_for(RegistryKey_t8752E3D024B69A6A9CC30AEB0E75F590C74E30D4_il2cpp_TypeInfo_var))->___s_hkeyNames = L_12;
		Il2CppCodeGenWriteBarrier((void**)(&((RegistryKey_t8752E3D024B69A6A9CC30AEB0E75F590C74E30D4_StaticFields*)il2cpp_codegen_static_fields_for(RegistryKey_t8752E3D024B69A6A9CC30AEB0E75F590C74E30D4_il2cpp_TypeInfo_var))->___s_hkeyNames), (void*)L_12);
		return;
	}
}
#ifdef __clang__
#pragma clang diagnostic pop
#endif
#ifdef __clang__
#pragma clang diagnostic push
#pragma clang diagnostic ignored "-Winvalid-offsetof"
#pragma clang diagnostic ignored "-Wunused-variable"
#endif
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR bool SafeRegistryHandle_ReleaseHandle_mC28FBF4CBE79E6368BA7BE49D7C4FA0EEA6BF845 (SafeRegistryHandle_t890BD43C81043709A2103F1FDC4394C603EE1FDF* __this, const RuntimeMethod* method) 
{
	{
		return (bool)1;
	}
}
#ifdef __clang__
#pragma clang diagnostic pop
#endif
IL2CPP_MANAGED_FORCE_INLINE IL2CPP_METHOD_ATTR bool SR_UsingResourceKeys_m36E934AE31A6845467D5FC45D6139D7CFFBAA0B6_inline (const RuntimeMethod* method) 
{
	static bool s_Il2CppMethodInitialized;
	if (!s_Il2CppMethodInitialized)
	{
		il2cpp_codegen_initialize_runtime_metadata((uintptr_t*)&SR_tCF5D02DC363D3707E4B0700773B397B107D749CF_il2cpp_TypeInfo_var);
		s_Il2CppMethodInitialized = true;
	}
	{
		il2cpp_codegen_runtime_class_init_inline(SR_tCF5D02DC363D3707E4B0700773B397B107D749CF_il2cpp_TypeInfo_var);
		bool L_0 = ((SR_tCF5D02DC363D3707E4B0700773B397B107D749CF_StaticFields*)il2cpp_codegen_static_fields_for(SR_tCF5D02DC363D3707E4B0700773B397B107D749CF_il2cpp_TypeInfo_var))->___s_usingResourceKeys;
		return L_0;
	}
}
IL2CPP_MANAGED_FORCE_INLINE IL2CPP_METHOD_ATTR int32_t String_get_Length_m42625D67623FA5CC7A44D47425CE86FB946542D2_inline (String_t* __this, const RuntimeMethod* method) 
{
	{
		int32_t L_0 = __this->____stringLength;
		return L_0;
	}
}
IL2CPP_MANAGED_FORCE_INLINE IL2CPP_METHOD_ATTR ObjectU5BU5D_t8061030B0A12A55D5AD8652A20C922FE99450918* Array_Empty_TisRuntimeObject_mFB8A63D602BB6974D31E20300D9EB89C6FE7C278_gshared_inline (const RuntimeMethod* method) 
{
	il2cpp_rgctx_method_init(method);
	{
		il2cpp_codegen_runtime_class_init_inline(il2cpp_rgctx_data(method->rgctx_data, 2));
		ObjectU5BU5D_t8061030B0A12A55D5AD8652A20C922FE99450918* L_0 = ((EmptyArray_1_tDF0DD7256B115243AA6BD5558417387A734240EE_StaticFields*)il2cpp_codegen_static_fields_for(il2cpp_rgctx_data(method->rgctx_data, 2)))->___Value;
		return L_0;
	}
}
