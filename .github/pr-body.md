Automated PR to update hl2sdk submodule.

## Changes
- **From:** `aba345d55e0d17fd1472bf321c192643906737b0`
- **To:** `394a726dcf4e805489c32cf90da50bb6e1b8a253`

## Commits in this update
```
394a726d Reset cvar/cmd reg list roots in ConVar_Unregister (#429)
fedf2673 Update CNetworkSerializerFieldInfo/ClassInfo
2fa8e053 Extend ConVar_Unregister to actually unregister all cvars/cmds
6315f010 Update SchemaClassFlags1_t & SchemaEnumFlags_t
79250346 Expose ConCommand/ConVarRegList to convar.h
99befe68 Update FTYPEDESC_* flags
625bfd4e Fix ScriptClassDesc_t declaration being missing (#428)
1fb66226 Update ScriptFunctionBinding_t & ScriptDataType_t (#427)
aeaa10b6 Update datamap_t, typedescription_t, CEntityClass, ComponentUnserializerClassInfo_t & INetworkGameServer vtable (#426)
de67e746 cs2: Update protobufs
5643d0ef Update INetworkGameServer vtable (#423)
8d169eed Update CEntityInstance vtable (#422)
d5801463 Update IFileSystem::String args
6d95a49f Update IFileSystem (#425)
8920c6d0 Update EntityIOQueuePrioritizedEvent_t
59c76ce2 Correct includes
bfa22d42 Update return type of CEntityInstance::GetScriptDesc
984726ae Update IRefCounted & fix missing includes on some headers (#421)
3ea69200 Add FCVAR_SNAPSHOT_IGNORED flag (#420)
936d985f Update CGlobalVarsBase (#419)
```

## Latest commit info
- **Message:** Reset cvar/cmd reg list roots in ConVar_Unregister (#429)
- **Author:** Michal
- **Date:** 2026-09-25 21:46:10 +0300

---
🤖 This PR was automatically created by the [bump-hl2sdk workflow](https://github.com/playpark/CounterStrikeSharp/actions/workflows/bump-hl2sdk.yml).
