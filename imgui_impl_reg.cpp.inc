
//----------------------------------------------------------------//
//----------------------------------------------------------------//
#undef IMGUI_FUNCTION
#define IMGUI_FUNCTION(name) {#name, impl_##name},
#undef IMGUI_FUNCTION_DRAW_LIST
#define IMGUI_FUNCTION_DRAW_LIST(name) {"DrawList_" #name, impl_draw_list_##name},
// These defines are just redefining everything to nothing so
// we can get the function names
#undef IM_TEXTURE_ID_ARG
#define IM_TEXTURE_ID_ARG(name)
#undef OPTIONAL_LABEL_ARG
#define OPTIONAL_LABEL_ARG(name, otherwise)
#undef LABEL_ARG
#define LABEL_ARG(name)
#undef IOTEXT_ARG
#define IOTEXT_ARG(name)
#undef IM_VEC_2_ARG
#define IM_VEC_2_ARG(name)
#undef IM_VEC_2_POINTER_ARG
#define IM_VEC_2_POINTER_ARG(name)
#undef OPTIONAL_IM_VEC_2_ARG
#define OPTIONAL_IM_VEC_2_ARG(name, x, y)
#undef IM_VEC_4_ARG
#define IM_VEC_4_ARG(name)
#undef IM_VEC_4_POINTER_ARG
#define IM_VEC_4_POINTER_ARG(name)
#undef OPTIONAL_IM_VEC_4_ARG
#define OPTIONAL_IM_VEC_4_ARG(name, x, y, z, w)
#undef NUMBER_ARG
#define NUMBER_ARG(name)
#undef NUMBER_POINTER_ARG
#define NUMBER_POINTER_ARG(name)
#undef END_NUMBER_POINTER
#define END_NUMBER_POINTER(name)
#undef FLOAT_ARG
#define FLOAT_ARG(name)
#undef OPTIONAL_FLOAT_ARG
#define OPTIONAL_FLOAT_ARG(name, otherwise)
#undef FLOAT_ARRAY_DEF
#define FLOAT_ARRAY_DEF(name, size)
#undef FLOAT_ARRAY_ARG
#define FLOAT_ARRAY_ARG(name, it)
#undef OPTIONAL_NUMBER_ARG
#define OPTIONAL_NUMBER_ARG(name, otherwise)
#undef FLOAT_POINTER_ARG
#define FLOAT_POINTER_ARG(name)
#undef END_FLOAT_POINTER
#define END_FLOAT_POINTER(name)
#undef OPTIONAL_INT_ARG
#define OPTIONAL_INT_ARG(name, otherwise)
#undef INT_ARG
#define INT_ARG(name)
#undef INT_ARRAY_DEF
#define INT_ARRAY_DEF(name,size)
#undef INT_ARRAY_ARG
#define INT_ARRAY_ARG(name,it)
#undef OPTIONAL_UINT_ARG
#define OPTIONAL_UINT_ARG(name, otherwise)
#undef UINT_ARG
#define UINT_ARG(name)
#undef INT_POINTER_ARG
#define INT_POINTER_ARG(name)
#undef END_INT_POINTER
#define END_INT_POINTER(name)
#undef UINT_POINTER_ARG
#define UINT_POINTER_ARG(name)
#undef END_UINT_POINTER
#define END_UINT_POINTER(name)
#undef BOOL_POINTER_ARG
#define BOOL_POINTER_ARG(name)
#undef OPTIONAL_BOOL_POINTER_ARG
#define OPTIONAL_BOOL_POINTER_ARG(name)
#undef OPTIONAL_BOOL_ARG
#define OPTIONAL_BOOL_ARG(name, otherwise)
#undef BOOL_ARG
#define BOOL_ARG(name)
#undef VOID_ARG
#define VOID_ARG(name)
#undef OPTIONAL_VOID_ARG
#define OPTIONAL_VOID_ARG(name, otherwise)
#undef LABEL_ARRAY_TABLE_ARG
#define LABEL_ARRAY_TABLE_ARG(name)
#undef CALLBACK_STUB
#define CALLBACK_STUB(name, callback)
#undef CALL_FUNCTION
#define CALL_FUNCTION(name, retType, ...)
#undef DRAW_LIST_CALL_FUNCTION
#define DRAW_LIST_CALL_FUNCTION(name, retType, ...)
#undef CALL_FUNCTION_NO_RET
#define CALL_FUNCTION_NO_RET(name, ...)
#undef DRAW_LIST_CALL_FUNCTION_NO_RET
#define DRAW_LIST_CALL_FUNCTION_NO_RET(name, ...)
#undef PUSH_STRING
#define PUSH_STRING(name)
#undef END_IOTEXT
#define END_IOTEXT(name)
#undef PUSH_NUMBER
#define PUSH_NUMBER(name)
#undef PUSH_BOOL
#define PUSH_BOOL(name)
#undef PUSH_TABLE
#define PUSH_TABLE
#undef PUSH_TABLE_TABLE
#define PUSH_TABLE_TABLE
#undef PUSH_TABLE_STRING
#define PUSH_TABLE_STRING(name)
#undef PUSH_TABLE_NUMBER
#define PUSH_TABLE_NUMBER(name)
#undef PUSH_TABLE_BOOL
#define PUSH_TABLE_BOOL(name)
#undef SET_TABLE_FIELD
#define SET_TABLE_FIELD(name)
#undef END_BOOL_POINTER
#define END_BOOL_POINTER(name)
#undef END_IMGUI_FUNC
#define END_IMGUI_FUNC
#undef END_STACK_START
#define END_STACK_START
#undef END_STACK_OPTION
#define END_STACK_OPTION(type, function)
#undef END_STACK_END
#define END_STACK_END
#undef IF_RET_ADD_END_STACK
#define IF_RET_ADD_END_STACK(type)
#undef ADD_END_STACK
#define ADD_END_STACK(type)
#undef POP_END_STACK
#define POP_END_STACK(type)
#undef START_ENUM
#define START_ENUM(name)
#undef MAKE_ENUM
#define MAKE_ENUM(c_name,lua_name)
#undef END_ENUM
#define END_ENUM(name)

#include "moai-imgui/imgui_iterator.cpp.inc"
