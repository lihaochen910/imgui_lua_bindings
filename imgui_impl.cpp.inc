
static int	g_textures[250]; // Should be enough
/*
** Wrapped functions
*/

#ifdef ENABLE_IM_LUA_END_STACK
// Stack for imgui begin and end
std::deque<int> endStack;
static void AddToStack(int type) {
    endStack.push_back(type);
}

static void PopEndStack(int type) {
    if (!endStack.empty()) {
        endStack.pop_back(); // hopefully the type matches
    }
}

static void ImEndStack(int type);

#endif

#define IMGUI_FUNCTION_DRAW_LIST(name) \
static int impl_draw_list_##name(lua_State *L) { \
    int max_args = lua_gettop(L); \
    int arg = 1; \
    int stackval = 0;

#define IMGUI_FUNCTION(name) \
static int impl_##name(lua_State *L) { \
    int max_args = lua_gettop(L); \
    int arg = 1; \
    int stackval = 0;

#define IMGUI

// I use OpenGL so this is a GLuint
// Using unsigned int cause im lazy don't copy me
#define IM_TEXTURE_ID_ARG(name) \
    const ImTextureID name = (ImTextureID)luaL_checkinteger(L, arg++);

#define LABEL_ARG(name) \
    size_t i_##name##_size; \
    const char * name = luaL_checklstring(L, arg++, &(i_##name##_size));

#define LABEL_ARRAY_DEF(name, size)\
    const char* name[size];

#define LABEL_ARRAY_ARG(name, it)\
    name[it] = luaL_checklstring(L, arg++, &(i_##name##_size));

#define OPTIONAL_LABEL_ARG(name, otherwise) \
    const char* name; \
    if (arg <= max_args) { \
        name = lua_tostring(L, arg++); \
    } else { \
        name = otherwise; \
    }

#define IOTEXT_ARG(name) \
    size_t i_##name##_size; \
    const char * i_##name##_const = luaL_checklstring(L, arg++, &(i_##name##_size)); \
    char name[255]; \
    strcpy(name, i_##name##_const);

#define END_IOTEXT(name) \
    const char* o_##name = name;\
    lua_pushstring(L, o_##name); \
    stackval++;

#define IM_VEC_2_ARG(name)\
    const lua_Number i_##name##_x = luaL_checknumber(L, arg++); \
    const lua_Number i_##name##_y = luaL_checknumber(L, arg++); \
    const ImVec2 name((double)i_##name##_x, (double)i_##name##_y);

#define IM_VEC_2_POINTER_ARG(name) \
    const lua_Number i_##name##_x = luaL_checknumber(L, arg++); \
    const lua_Number i_##name##_y = luaL_checknumber(L, arg++); \
    const ImVec2 i_##name##_value((double)i_##name##_x, (double)i_##name##_y); \
    const ImVec2* name = &(i_##name##_value);

#define OPTIONAL_IM_VEC_2_ARG(name, x, y) \
    lua_Number i_##name##_x = x; \
    lua_Number i_##name##_y = y; \
    if (arg <= max_args - 1) { \
        i_##name##_x = luaL_checknumber(L, arg++); \
        i_##name##_y = luaL_checknumber(L, arg++); \
    } \
    const ImVec2 name((double)i_##name##_x, (double)i_##name##_y);

#define IM_VEC_4_ARG(name) \
    const lua_Number i_##name##_x = luaL_checknumber(L, arg++); \
    const lua_Number i_##name##_y = luaL_checknumber(L, arg++); \
    const lua_Number i_##name##_z = luaL_checknumber(L, arg++); \
    const lua_Number i_##name##_w = luaL_checknumber(L, arg++); \
    const ImVec4 name((double)i_##name##_x, (double)i_##name##_y, (double)i_##name##_z, (double)i_##name##_w);

#define IM_VEC_4_POINTER_ARG(name) \
    const lua_Number i_##name##_x = luaL_checknumber(L, arg++); \
    const lua_Number i_##name##_y = luaL_checknumber(L, arg++); \
    const lua_Number i_##name##_z = luaL_checknumber(L, arg++); \
    const lua_Number i_##name##_w = luaL_checknumber(L, arg++); \
    const ImVec4 i_##name##_value((double)i_##name##_x, (double)i_##name##_y, (double)i_##name##_z, (double)i_##name##_w); \
    const ImVec4* name = &(i_##name##_value);

#define OPTIONAL_IM_VEC_4_ARG(name, x, y, z, w) \
    lua_Number i_##name##_x = x; \
    lua_Number i_##name##_y = y; \
    lua_Number i_##name##_z = z; \
    lua_Number i_##name##_w = w; \
    if (arg <= max_args - 1) { \
        i_##name##_x = luaL_checknumber(L, arg++); \
        i_##name##_y = luaL_checknumber(L, arg++); \
        i_##name##_z = luaL_checknumber(L, arg++); \
        i_##name##_w = luaL_checknumber(L, arg++); \
    } \
    const ImVec4 name((double)i_##name##_x, (double)i_##name##_y, (double)i_##name##_z, (double)i_##name##_w);

#define NUMBER_ARG(name)\
    lua_Number name = luaL_checknumber(L, arg++);

#define NUMBER_POINTER_ARG(name) \
    lua_Number i_##name##_value = luaL_checknumber(L, arg++); \
    lua_Number* name = &(i_##name##_value);

#define END_NUMBER_POINTER(name) \
    if (name != NULL) { \
        lua_pushnumber(L, i_##name##_value); \
        stackval++; \
    }

#define FLOAT_ARG(name)\
    float name = luaL_checknumber(L, arg++);

#define OPTIONAL_FLOAT_ARG(name, otherwise)\
    float name = otherwise; \
    if (arg <= max_args) { \
        name = (float)luaL_checknumber(L, arg++); \
    }

#define FLOAT_ARRAY_DEF(name, size)\
    float name[size];

#define FLOAT_ARRAY_ARG(name, it)\
    name[it] = (float)luaL_checknumber(L, arg++);

#define OPTIONAL_NUMBER_ARG(name, otherwise)\
    lua_Number name = otherwise; \
    if (arg <= max_args) { \
        name = lua_tonumber(L, arg++); \
    }

#define FLOAT_POINTER_ARG(name) \
    float i_##name##_value = luaL_checknumber(L, arg++); \
    float* name = &(i_##name##_value);

#define END_FLOAT_POINTER(name) \
    if (name != NULL) { \
        lua_pushnumber(L, i_##name##_value); \
        stackval++; \
    }

#define INT_ARG(name) \
    const int name = (int)luaL_checknumber(L, arg++);

#define INT_ARRAY_DEF(name,size) \
    int name[size];

#define INT_ARRAY_ARG(name,it) \
    name[it] = (int)luaL_checknumber(L, arg++);

#define OPTIONAL_INT_ARG(name, otherwise)\
    int name = otherwise; \
    if (arg <= max_args) { \
        name = (int)lua_tonumber(L, arg++); \
    }

#define INT_POINTER_ARG(name) \
    int i_##name##_value = (int)luaL_checkinteger(L, arg++); \
    int* name = &(i_##name##_value);

#define END_INT_POINTER(name) \
    if (name != NULL) { \
        lua_pushnumber(L, i_##name##_value); \
        stackval++; \
    }

#define UINT_ARG(name) \
    const unsigned int name = (unsigned int)luaL_checkinteger(L, arg++);

#define OPTIONAL_UINT_ARG(name, otherwise)\
    unsigned int name = otherwise; \
    if (arg <= max_args) { \
        name = (unsigned int)luaL_checkinteger(L, arg++); \
    }

#define UINT_POINTER_ARG(name) \
    unsigned int i_##name##_value = (unsigned int)luaL_checkinteger(L, arg++); \
    unsigned int* name = &(i_##name##_value);

#define END_UINT_POINTER(name) \
    if (name != NULL) { \
        lua_pushnumber(L, i_##name##_value); \
        stackval++; \
    }

#define BOOL_ARG(name) \
    bool name = lua_toboolean(L, arg++);

#define OPTIONAL_BOOL_ARG(name, otherwise) \
    bool name = otherwise; \
    if (arg <= max_args) { \
        name = lua_toboolean(L, arg++); \
    }

#define BOOL_POINTER_ARG(name) \
    bool i_##name##_value = lua_toboolean(L, arg++); \
    bool* name = &(i_##name##_value);

#define OPTIONAL_BOOL_POINTER_ARG(name) \
    bool i_##name##_value; \
    bool* name = NULL; \
    if (arg <= max_args) { \
        i_##name##_value = lua_toboolean(L, arg++); \
        name = &(i_##name##_value); \
    }

#define END_BOOL_POINTER(name) \
    if (name != NULL) { \
        lua_pushboolean(L, (int)i_##name##_value); \
        stackval++; \
    }

#define VOID_ARG(name) \
    void* name = NULL; \
    size_t arg_##name = arg++; \
    lua_Number o_##name##_int; \
    bool o_##name##_bool; \
    char* o_##name##_str; \
    int type_##name = lua_type(L, arg_##name); \
    switch (type_##name) { \
        case LUA_TNUMBER: \
        { \
            o_##name##_int = luaL_checknumber(L, arg_##name); \
            name = (void*)&o_##name##_int; \
            break; \
        } \
        case LUA_TBOOLEAN: \
        { \
            o_##name##_bool = lua_toboolean(L, arg_##name); \
            name = (void*)&o_##name##_bool; \
            break; \
        } \
        case LUA_TSTRING: \
        { \
            size_t i_##name##_size; \
            o_##name##_str = const_cast<char*>(luaL_checklstring(L, arg_##name, &(i_##name##_size))); \
            name = (void*)&o_##name##_str; \
            break; \
        } \
    }

#define OPTIONAL_VOID_ARG(name, otherwise) \
    void* name = NULL; \
    size_t arg_##name = arg++; \
    if (arg_##name <= max_args) { \
        lua_Number o_##name##_int; \
        bool o_##name##_bool; \
        char* o_##name##_str; \
        int type_##name = lua_type(L, arg_##name); \
        switch (type_##name) { \
            case LUA_TNUMBER: \
            { \
                o_##name##_int = luaL_checknumber(L, arg_##name); \
                name = (void*)&o_##name##_int; \
                break; \
            } \
            case LUA_TBOOLEAN: \
            { \
                o_##name##_bool = lua_toboolean(L, arg_##name); \
                name = (void*)&o_##name##_bool; \
                break; \
            } \
            case LUA_TSTRING: \
            { \
                size_t i_##name##_size; \
                o_##name##_str = const_cast<char*>(luaL_checklstring(L, arg_##name, &(i_##name##_size))); \
                name = (void*)&o_##name##_str; \
                break; \
            } \
        } \
    }

// printf( "LABEL_ARRAY_TABLE_ARG: %d %s\n", i, str );
#define LABEL_ARRAY_TABLE_ARG(name) \
    luaL_checktype(L, arg, LUA_TTABLE); \
    int n = (int)lua_objlen(L, arg); \
    const char* name[n]; \
    for ( int i = 1; i <= n; ++i ) { \
        lua_pushnumber(L, i); \
        lua_gettable(L, arg); \
        const char* str = lua_tostring(L, arg+2); \
        name[ i - 1 ] = str; \
        lua_pop(L, 1); \
    } \
    arg++;

#define CALLBACK_STUB(name, callback) \
    callback name = NULL;

#define CALL_FUNCTION(name, retType,...) \
    retType ret = ImGui::name(__VA_ARGS__);

#define DRAW_LIST_CALL_FUNCTION(name, retType,...) \
    retType ret = ImGui::GetWindowDrawList()->name(__VA_ARGS__);

#define CALL_FUNCTION_NO_RET(name, ...) \
    ImGui::name(__VA_ARGS__);

#define DRAW_LIST_CALL_FUNCTION_NO_RET(name, ...) \
    ImGui::GetWindowDrawList()->name(__VA_ARGS__);

#define PUSH_STRING(name) \
    lua_pushstring(L, name); \
    stackval++;

#define PUSH_NUMBER(name) \
    lua_pushnumber(L, name); \
    stackval++;

#define PUSH_BOOL(name) \
    lua_pushboolean(L, (int) name); \
    stackval++;

#define PUSH_TABLE \
    lua_newtable(L); \
    stackval++;

#define PUSH_TABLE_TABLE \
    lua_newtable(L); 

#define PUSH_TABLE_STRING(name) \
    lua_pushstring(L, name); 

#define PUSH_TABLE_NUMBER(name) \
    lua_pushnumber(L, name); 

#define PUSH_TABLE_BOOL(name) \
    lua_pushboolean(L, (int) name); 

#define SET_TABLE_FIELD(name) \
    lua_setfield(L, -2, name);

#define END_IMGUI_FUNC \
    return stackval; \
}

#ifdef ENABLE_IM_LUA_END_STACK
#define IF_RET_ADD_END_STACK(type) \
    if (ret) { \
        AddToStack(type); \
    }

#define ADD_END_STACK(type) \
    AddToStack(type);

#define POP_END_STACK(type) \
    PopEndStack(type);

#define END_STACK_START \
static void ImEndStack(int type) { \
    switch(type) {

#define END_STACK_OPTION(type, function) \
        case type: \
            ImGui::function(); \
            break;

#define END_STACK_END \
    } \
}
#else
#define END_STACK_START
#define END_STACK_OPTION(type, function)
#define END_STACK_END
#define IF_RET_ADD_END_STACK(type)
#define ADD_END_STACK(type)
#define POP_END_STACK(type)
#endif

#define START_ENUM(name)
#define MAKE_ENUM(c_name,lua_name)
#define END_ENUM(name)

#include "moai-imgui/imgui_iterator.cpp.inc"
