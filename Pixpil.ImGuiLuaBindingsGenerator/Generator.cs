// #define LOG_TODO
using System.Text;
using CppAst;


namespace Pixpil.ImGuiLuaBindingsGenerator;

public static class Generator {
	
	public const int TargetImguiVersionNum = 18946;
	
	public static string[] KSupportedTypedefs = {
		"ImGuiKey",
		
		"ImGuiCol",
		"ImGuiCond",
		"ImGuiDataType",
		"ImGuiDir",
		"ImGuiMouseButton",
		"ImGuiMouseCursor",
		"ImGuiSortDirection",
		"ImGuiStyleVar",
		"ImGuiTableBgTarget",
		
		"ImDrawFlags",
		"ImDrawListFlags",
		"ImFontAtlasFlags",
		"ImGuiBackendFlags",
		"ImGuiButtonFlags",
		"ImGuiColorEditFlags",
		"ImGuiConfigFlags",
		"ImGuiComboFlags",
		"ImGuiDragDropFlags",
		"ImGuiFocusedFlags",
		"ImGuiHoveredFlags",
		"ImGuiInputTextFlags",
		"ImGuiKeyChord",
		"ImGuiPopupFlags",
		"ImGuiSelectableFlags",
		"ImGuiSliderFlags",
		"ImGuiTabBarFlags",
		"ImGuiTabItemFlags",
		"ImGuiTableFlags",
		"ImGuiTableColumnFlags",
		"ImGuiTableRowFlags",
		"ImGuiTreeNodeFlags",
		"ImGuiViewportFlags",
		"ImGuiWindowFlags",
		
		// "ImGuiID",
		"ImS8",
		"ImU8",
		"ImS16",
		"ImU16",
		"ImS32",
		// "ImU32",
		"ImS64",
		"ImU64",
		
		"ImWchar16",
		"ImWchar32"
	};
	
	public static string[] KSupportedUIntTypedefs = {
		"ImGuiID",
		"ImU32",
		"size_t"
	};

	public static string[] KSupportedTypedefCallbacks = {
		"ImGuiInputTextCallback",
		"ImGuiSizeCallback",
		"ImGuiMemAllocFunc",
		"ImGuiMemFreeFunc",
	};

	public static string[] KSupportedEnums = {
		"ImGuiKey"
	};
	
	public static string[] KSupportedStructs = {
		"ImColor",
		"ImVec2",
		"ImVec4"
	};

	public const string KConstCharPointer = "const char*";
	public const string KSizeT = "size_t";
	public const string KStructImVec2 = "ImVec2";
	public const string KConstPointerImVec2 = "const ImVec2*";
	public const string KConstRefImVec2 = "const ImVec2&";
	public const string KStructImVec4 = "ImVec4";
	public const string KConstPointerImVec4 = "const ImVec4*";
	public const string KConstRefImVec4 = "const ImVec4&";
	public const string KStructImColor = "ImColor";
	public const string KTypedefImTextureID = "ImTextureID";


	private static Dictionary< CppFunction, string > OverloadCppFunctionNames = new Dictionary< CppFunction , string >();

	public static string Parse( string imguiPath ) {
		
		CppParserOptions options = new CppParserOptions();
		if ( Environment.OSVersion.Platform == PlatformID.Unix ) {
			options.TargetSystem = "darwin";
			options.TargetCpu = CppTargetCpu.ARM64;
			options.IncludeFolders.Add( @"/opt/homebrew/Cellar/llvm/16.0.0/lib/clang/16/include" );
			options.IncludeFolders.Add( @"/opt/homebrew/Cellar/llvm/16.0.0/include/c++/v1" );
			options.IncludeFolders.Add( @"/Library/Developer/CommandLineTools/SDKs/MacOSX.sdk/usr/include" );
		}
		options.IncludeFolders.Add( imguiPath );
		CppCompilation compilation = CppParser.ParseFiles(
			new List< string > {
				// Path.Combine( imguiPath, "imconfig.h" ),
				Path.Combine( imguiPath, "imgui.h" ),
				// Path.Combine( imguiPath, "imgui_internal.h" )
			},
			options );

		if ( compilation.HasErrors ) {
			foreach ( var msg in compilation.Diagnostics.Messages ) {
				Console.WriteLine( $"{msg.Text}" );
			}
			return string.Empty;
		}

		List< string > parsedFunctionNames = new List< string >();
		StringBuilder output = new StringBuilder();
		output.AppendLine( "// this is a generated file, DO NOT EDIT\n" );

		int imguiFunctionCount = 0;
		
		CppMacro versionMacroDefine = compilation.Macros.Where( ma => ma.Name == "IMGUI_VERSION" ).FirstOrDefault();
		if ( versionMacroDefine != null ) {
			output.AppendLine( $"// IMGUI_VERSION: {versionMacroDefine.Value}" );
		}
		versionMacroDefine = compilation.Macros.Where( ma => ma.Name == "IMGUI_VERSION_NUM" ).FirstOrDefault();
		if ( versionMacroDefine != null ) {
			if ( Convert.ToInt32( versionMacroDefine.Value ) > TargetImguiVersionNum ) {
				Console.WriteLine( $"[Warning] IMGUI_VERSION_NUM > Generator.TargetImguiVersionNum {versionMacroDefine.Value}" );
			}
			output.AppendLine( $"// IMGUI_VERSION_NUM: {versionMacroDefine.Value}\n" );
		}
		
		CppNamespace ns = compilation.Namespaces.Where( ns => ns.Name == "ImGui" ).FirstOrDefault();

		if ( ns != null ) {
			output.AppendLine( "/*" );
			foreach ( CppEnum @enum in compilation.Enums ) {
				WriteBindingEnum( @enum, output );
			}
			output.AppendLine( "*/\n" );

			CollectOverloadFunctions( ns );
			foreach ( CppFunction func in ns.Functions ) {
				if ( MatchImguiApiCppFunction( func ) ) {
					// Console.WriteLine( $"{ns.Name} func {func.Name}" );
					imguiFunctionCount++;
					if ( WriteBindingFunction( func, output ) ) {
						parsedFunctionNames.Add( func.Name );
					}
				}
				else {
#if LOG_TODO
					Console.WriteLine( $"CppFunction not support: {ns.Name} func {func.Name}" );
#endif
				}
			}
		}

		// foreach ( CppFunction func in ns.Functions ) {
		// 	Console.WriteLine( $"func {func.Name} {func.FullParentName}" );
		// }
		
		Console.WriteLine( $"Supported: {parsedFunctionNames.Count}/{imguiFunctionCount}" );
		// Console.WriteLine( output.ToString() );

		return output.ToString();
	}
	
	private static bool MatchImguiApiCppFunction( CppFunction function ) {
		bool functionLinkageKindPass = false;
		if ( function.LinkageKind == CppLinkageKind.External ||
		     function.LinkageKind == CppLinkageKind.UniqueExternal ) {
			functionLinkageKindPass = true;
		}

		bool functionReturnTypePass = true;

		// if ( function.ReturnType is CppPrimitiveType ||
		//      ( function.ReturnType.TypeKind == CppTypeKind.Typedef && KSupportedTypedefs.Contains( function.ReturnType.GetDisplayName() ) ) ||
		//      ( function.ReturnType.TypeKind == CppTypeKind.Enum && KSupportedEnums.Contains( function.ReturnType.GetDisplayName() ) ) ||
		//      ( function.ReturnType.TypeKind == CppTypeKind.StructOrClass && KSupportedStructs.Contains( function.ReturnType.GetDisplayName() ) ) ||
		//      ( function.ReturnType.GetDisplayName() == KConstCharPointer )
		//     ) {
		// 	functionReturnTypePass = true;
		// }
		
		return functionLinkageKindPass && functionReturnTypePass;
	}

	private static void CollectOverloadFunctions( CppNamespace cppNamespace ) {
		OverloadCppFunctionNames.Clear();
		Dictionary< string, List< CppFunction > > dictionary = new Dictionary< string , List< CppFunction > >();

		StringBuilder temp = new StringBuilder();
		
		foreach ( CppFunction func in cppNamespace.Functions ) {
			if ( !MatchImguiApiCppFunction( func ) ) {
				continue;
			}
			if ( !WriteBindingFunction( func, temp ) ) {
				continue;
			}
			if ( !dictionary.ContainsKey( func.Name ) ) {
				dictionary.Add( func.Name, new List< CppFunction >() );
			}
			dictionary[ func.Name ].Add( func );
		}

		foreach ( var funcName in dictionary.Keys ) {
			if ( dictionary[ funcName ].Count <= 1 ) {
				dictionary.Remove( funcName );
			}
		}

		foreach ( var funcs in dictionary.Values ) {
			for ( int i = 0; i < funcs.Count; i++ ) {
				if ( i == 0 ) {
					OverloadCppFunctionNames.Add( funcs[ i ], funcs[ i ].Name );
				}
				else {
					OverloadCppFunctionNames.Add( funcs[ i ], $"{funcs[ i ].Name}_{i + 1}" );
				}
			}
		}
	}

	private static void WriteBindingEnum( CppEnum @enum, StringBuilder builder ) {
		// builder.Append( $"//----------------------------------------------------------------//\n" );
		// builder.Append( $"// {@enum.Name}\n" );
		// builder.Append( $"//----------------------------------------------------------------//\n" );
		// foreach ( var enumItem in @enum.Items ) {
		// 	builder.Append( $"// {enumItem.Name}\n" );
		// }
		// builder.AppendLine();

		foreach ( var enumItem in @enum.Items ) {
			builder.AppendLine( $"state.SetField( -1, \"{enumItem.Name.Replace( "ImGui", string.Empty )}\", {enumItem.Name} );" );
		}
	}

	private static bool WriteBindingFunction( CppFunction function, StringBuilder builder ) {

		bool supported = true;
		
		string callPrefix = string.Empty;
		string functionSuffix = string.Empty;
		string luaFunc = !OverloadCppFunctionNames.ContainsKey( function ) ? function.Name : OverloadCppFunctionNames[ function ];
		string callMacro = string.Empty;
		string after = string.Empty;
		string before = string.Empty;
		string funcName = function.Name;
		List< string > funcArgs = new List< string >( function.Parameters.Count );

		void PushFuncArg( string argType ) => funcArgs.Add( argType );
		
		void WriteFunctionReturn() {
			switch ( function.ReturnType.TypeKind ) {
				case CppTypeKind.Primitive:
					if ( function.ReturnType.Equals( CppPrimitiveType.Void ) ) {
						callMacro = $"{callPrefix}CALL_FUNCTION_NO_RET";
					}
					else if ( function.ReturnType.Equals( CppPrimitiveType.Bool ) ) {
						callMacro = $"{callPrefix}CALL_FUNCTION";
						PushFuncArg( function.ReturnType.GetDisplayName() );
						after += "PUSH_BOOL( ret )\n";
					}
					else if ( function.ReturnType.Equals( CppPrimitiveType.Int ) ||
					          function.ReturnType.Equals( CppPrimitiveType.UnsignedInt ) ||
					          function.ReturnType.Equals( CppPrimitiveType.Short ) ||
					          function.ReturnType.Equals( CppPrimitiveType.UnsignedShort ) ||
					          function.ReturnType.Equals( CppPrimitiveType.Float ) ||
					          function.ReturnType.Equals( CppPrimitiveType.Double ) ) {
						callMacro = $"{callPrefix}CALL_FUNCTION";
						PushFuncArg( function.ReturnType.GetDisplayName() );
						after += "PUSH_NUMBER( ret )\n";
					}
					else {
						goto Unsupported;
					}
					break;
				case CppTypeKind.Pointer:
					if ( function.ReturnType.GetDisplayName() == KConstCharPointer ) {
						callMacro = $"{callPrefix}CALL_FUNCTION";
						PushFuncArg( KConstCharPointer );
						after += "PUSH_STRING( ret )\n";
					}
					else {
						goto Unsupported;
					}
					break;
				case CppTypeKind.Reference:
					if ( function.ReturnType.GetDisplayName() == KConstRefImVec2 ) {
						callMacro = $"{callPrefix}CALL_FUNCTION";
						PushFuncArg( KStructImVec2 );
						after += "PUSH_TABLE\n";
						after += "PUSH_TABLE_NUMBER( ret.x )\n";
						after += "SET_TABLE_FIELD( \"x\" )\n";
						after += "PUSH_TABLE_NUMBER( ret.y )\n";
						after += "SET_TABLE_FIELD( \"y\" )\n";
					}
					if ( function.ReturnType.GetDisplayName() == KConstRefImVec4 ) {
						callMacro = $"{callPrefix}CALL_FUNCTION";
						PushFuncArg( KStructImVec4 );
						after += "PUSH_TABLE\n";
						after += "PUSH_TABLE_NUMBER( ret.x )\n";
						after += "SET_TABLE_FIELD( \"x\" )\n";
						after += "PUSH_TABLE_NUMBER( ret.y )\n";
						after += "SET_TABLE_FIELD( \"y\" )\n";
						after += "PUSH_TABLE_NUMBER( ret.z )\n";
						after += "SET_TABLE_FIELD( \"z\" )\n";
						after += "PUSH_TABLE_NUMBER( ret.w )\n";
						after += "SET_TABLE_FIELD( \"w\" )\n";
					}
					else {
						goto Unsupported;
					}
					break;
				case CppTypeKind.StructOrClass:
					if ( function.ReturnType.GetDisplayName() == KStructImVec2 ) {
						callMacro = $"{callPrefix}CALL_FUNCTION";
						PushFuncArg( KStructImVec2 );
						after += "PUSH_TABLE\n";
						after += "PUSH_TABLE_NUMBER( ret.x )\n";
						after += "SET_TABLE_FIELD( \"x\" )\n";
						after += "PUSH_TABLE_NUMBER( ret.y )\n";
						after += "SET_TABLE_FIELD( \"y\" )\n";
					}
					else if ( function.ReturnType.GetDisplayName() == KStructImVec4 ) {
						callMacro = $"{callPrefix}CALL_FUNCTION";
						PushFuncArg( KStructImVec4 );
						after += "PUSH_TABLE\n";
						after += "PUSH_TABLE_NUMBER( ret.x )\n";
						after += "SET_TABLE_FIELD( \"x\" )\n";
						after += "PUSH_TABLE_NUMBER( ret.y )\n";
						after += "SET_TABLE_FIELD( \"y\" )\n";
						after += "PUSH_TABLE_NUMBER( ret.z )\n";
						after += "SET_TABLE_FIELD( \"z\" )\n";
						after += "PUSH_TABLE_NUMBER( ret.w )\n";
						after += "SET_TABLE_FIELD( \"w\" )\n";
					}
					else {
						goto Unsupported;
					}
					break;
				case CppTypeKind.Enum:
					if ( KSupportedEnums.Contains( function.ReturnType.GetDisplayName() ) ) {
						callMacro = $"{callPrefix}CALL_FUNCTION";
						PushFuncArg( "unsigned int" );
						after += "PUSH_NUMBER( ret )\n";
					}
					else {
						goto Unsupported;
					}
					break;
				case CppTypeKind.Typedef:
					if ( KSupportedTypedefs.Contains( function.ReturnType.GetDisplayName() ) ||
					     function.ReturnType.GetDisplayName() == KSizeT ) {
						callMacro = $"{callPrefix}CALL_FUNCTION";
						PushFuncArg( "int" );
						after += "PUSH_NUMBER( ret )\n";
					}
					else {
						goto Unsupported;
					}
					break;
				default:
					Unsupported:
					supported = false;
#if LOG_TODO
					Console.WriteLine( $"TODO: unsupported {function.ReturnType.TypeKind} ReturnType {function.ReturnType}" );
#endif
					break;
			}
		}
		
		void WriteFunctionParam( CppParameter parameter ) {
			bool parameterSupported = true;
			switch ( parameter.Type.TypeKind ) {
				case CppTypeKind.Primitive:
					if ( parameter.Type.Equals( CppPrimitiveType.Bool ) ) {
						if ( parameter.InitValue != null ) {
							bool value = ( long )parameter.InitValue.Value == 0 ? false : true;
							before += $"OPTIONAL_BOOL_ARG( {parameter.Name}, {value.ToString().ToLower()} )\n";
						}
						else {
							before += $"BOOL_ARG( {parameter.Name} )\n";
						}
					}
					else if ( parameter.Type.Equals( CppPrimitiveType.Int ) ) {
						if ( parameter.InitValue != null ) {
							before += $"OPTIONAL_INT_ARG( {parameter.Name}, {parameter.InitValue.Value} )\n";
						}
						else {
							before += $"INT_ARG( {parameter.Name} )\n";
						}
					}
					else if ( parameter.Type.Equals( CppPrimitiveType.Float ) ) {
						if ( parameter.InitValue != null ) {
							before += $"OPTIONAL_FLOAT_ARG( {parameter.Name}, {parameter.InitValue.Value} )\n";
						}
						else {
							before += $"FLOAT_ARG( {parameter.Name} )\n";
						}
					}
					else if ( parameter.Type.Equals( CppPrimitiveType.UnsignedInt ) ) {
						if ( parameter.InitValue != null ) {
							before += $"OPTIONAL_UINT_ARG( {parameter.Name}, {parameter.InitValue.Value} )\n";
						}
						else {
							before += $"UINT_ARG( {parameter.Name} )\n";
						}
					}
					else if ( parameter.Type.Equals( CppPrimitiveType.Short ) ||
					          parameter.Type.Equals( CppPrimitiveType.UnsignedShort ) ||
					          parameter.Type.Equals( CppPrimitiveType.Float ) ||
					          parameter.Type.Equals( CppPrimitiveType.Double ) ) {
						if ( parameter.InitValue != null ) {
							before += $"OPTIONAL_NUMBER_ARG( {parameter.Name}, {parameter.InitValue.Value} )\n";
						}
						else {
							before += $"NUMBER_ARG( {parameter.Name} )\n";
						}
					}
					else {
						goto Unsupported;
					}
					break;
				case CppTypeKind.StructOrClass:
					if ( parameter.Type.GetDisplayName() == KStructImVec2 ) {
						before += $"IM_VEC_2_ARG( {parameter.Name} )\n";
					}
					else if ( parameter.Type.GetDisplayName() == KStructImVec4 ) {
						before += $"IM_VEC_4_ARG( {parameter.Name} )\n";
					}
					else {
						goto Unsupported;
					}
					break;
				case CppTypeKind.Pointer:
					var pointerType = ( CppPointerType )parameter.Type;
					if ( pointerType.ElementType.Equals( CppPrimitiveType.Bool ) ||
					     pointerType.ElementType is CppQualifiedType && ((CppQualifiedType)pointerType.ElementType).ElementType.Equals( CppPrimitiveType.Bool ) ) {
						if ( parameter.InitValue != null ) {
							before += $"OPTIONAL_BOOL_POINTER_ARG( {parameter.Name} )\n";
						}
						else {
							before += $"BOOL_POINTER_ARG( {parameter.Name} )\n";
						}
						after += $"END_BOOL_POINTER( {parameter.Name} )\n";
					}
					else if ( pointerType.ElementType.Equals( CppPrimitiveType.Float ) ||
							  pointerType.ElementType is CppQualifiedType && ((CppQualifiedType)pointerType.ElementType).ElementType.Equals( CppPrimitiveType.Float ) ) {
						before += $"FLOAT_POINTER_ARG( {parameter.Name} )\n";
						after += $"END_FLOAT_POINTER( {parameter.Name} )\n";
					}
					else if ( pointerType.ElementType.Equals( CppPrimitiveType.Double ) ||
							  pointerType.ElementType is CppQualifiedType && ((CppQualifiedType)pointerType.ElementType).ElementType.Equals( CppPrimitiveType.Double ) ) {
						before += $"NUMBER_POINTER_ARG( {parameter.Name} )\n";
						after += $"END_NUMBER_POINTER( {parameter.Name} )\n";
					}
					// int * x
					else if ( pointerType.ElementType.Equals( CppPrimitiveType.Int ) ||
							  pointerType.ElementType is CppQualifiedType && ((CppQualifiedType)pointerType.ElementType).ElementType.Equals( CppPrimitiveType.Int ) ) {
						before += $"INT_POINTER_ARG( {parameter.Name} )\n";
						after += $"END_INT_POINTER( {parameter.Name} )\n";
					}
					// unsigned int * x
					else if ( pointerType.ElementType.Equals( CppPrimitiveType.UnsignedInt ) ||
							  pointerType.ElementType is CppQualifiedType && ((CppQualifiedType)pointerType.ElementType).ElementType.Equals( CppPrimitiveType.UnsignedInt ) ) {
						before += $"UINT_POINTER_ARG( {parameter.Name} )\n";
						after += $"END_UINT_POINTER( {parameter.Name} )\n";
					}
					// returnable char* a or char* a = NULL
					else if ( pointerType.ElementType.Equals( CppPrimitiveType.Char ) ) {
						before += $"IOTEXT_ARG( {parameter.Name} )\n";
						after += $"END_IOTEXT( {parameter.Name} )\n";
					}
					// const void* or void * a types and can have default value or not
					else if ( pointerType.ElementType.Equals( CppPrimitiveType.Void ) ||
							  pointerType.ElementType is CppQualifiedType && ((CppQualifiedType)pointerType.ElementType).ElementType.Equals( CppPrimitiveType.Void ) ) {
						if ( parameter.InitValue != null ) {
							before += $"OPTIONAL_VOID_ARG( {parameter.Name}, {parameter.InitValue} )\n";
						}
						else if ( parameter.InitExpression != null ) {
							var rawExpression = parameter.InitExpression as CppRawExpression;
							var expressionValue = string.IsNullOrEmpty( rawExpression.Text ) ? "0" : rawExpression.Text;
							before += $"OPTIONAL_VOID_ARG( {parameter.Name}, {expressionValue} )\n";
						}
						else {
							before += $"VOID_ARG( {parameter.Name} )\n";
						}
					}
					else if ( parameter.Type.GetDisplayName() == KConstCharPointer ) {
						if ( parameter.InitValue != null ) {
							before += $"OPTIONAL_LABEL_ARG( {parameter.Name}, \"{parameter.InitValue}\" )\n";
						}
						else if ( parameter.InitExpression != null ) {
							before += $"OPTIONAL_LABEL_ARG( {parameter.Name}, \"{((CppRawExpression)parameter.InitExpression).Text}\" )\n";
						}
						else {
							before += $"LABEL_ARG( {parameter.Name} )\n";
						}
					}
					else if ( parameter.Type.GetDisplayName() == KConstPointerImVec2 ) {
						before += $"IM_VEC_2_POINTER_ARG( {parameter.Name} )\n";
					}
					else if ( parameter.Type.GetDisplayName() == KConstPointerImVec4 ) {
						before += $"IM_VEC_4_POINTER_ARG( {parameter.Name} )\n";
					}
					else {
						goto Unsupported;
					}
					break;
				case CppTypeKind.Reference:
					var referenceType = ( CppReferenceType )parameter.Type;
					if ( referenceType.ElementType.Equals( CppPrimitiveType.Bool ) ||
					     referenceType.ElementType is CppQualifiedType && ((CppQualifiedType)referenceType.ElementType).ElementType.Equals( CppPrimitiveType.Bool ) ) {
						if ( parameter.InitValue != null ) {
							before += $"OPTIONAL_BOOL_ARG( {parameter.Name}, {parameter.InitValue} )\n";
						}
						else {
							before += $"BOOL_ARG( {parameter.Name} )\n";
						}
					}
					else if ( referenceType.ElementType.Equals( CppPrimitiveType.Float ) ) {
						if ( parameter.InitValue != null ) {
							before += $"OPTIONAL_NUMBER_ARG( {parameter.Name}, {parameter.InitValue} )\n";
						}
						else {
							before += $"FLOAT_ARG( {parameter.Name} )\n";
						}
					}
					else if ( referenceType.ElementType.Equals( CppPrimitiveType.Int ) ||
					          referenceType.ElementType.Equals( CppPrimitiveType.UnsignedInt ) ||
					          referenceType.ElementType.Equals( CppPrimitiveType.Short ) ||
					          referenceType.ElementType.Equals( CppPrimitiveType.UnsignedShort ) ||
					          referenceType.ElementType.Equals( CppPrimitiveType.Double ) ) {
						if ( parameter.InitValue != null ) {
							before += $"OPTIONAL_NUMBER_ARG( {parameter.Name}, {parameter.InitValue} )\n";
						}
						else {
							before += $"NUMBER_ARG( {parameter.Name} )\n";
						}
					}
					else if ( referenceType.GetDisplayName() == KStructImVec2 ||
					          referenceType.ElementType is CppQualifiedType && ((CppQualifiedType)referenceType.ElementType).ElementType.GetDisplayName() == KStructImVec2 ) {
						if ( parameter.InitValue != null || parameter.InitExpression != null ) {
							var express =
								( parameter.InitExpression as CppRawExpression ).Text
									.Replace( KStructImVec2, string.Empty )
									.Replace( "(", string.Empty )
									.Replace( ")", string.Empty )
									.Replace( 'f', char.MinValue )
									.Replace( "-FLT_MIN", float.MinValue.ToString() )
									.Replace( "FLT_MAX", float.MaxValue.ToString() )
									.Trim()
									.Split( ',' );
							float x = 0, y = 0;
							if ( express.Length > 0 ) {
								try {
									// float.TryParse( express[ 0 ], out x );
									// float.TryParse( express[ 1 ], out y );
									x = float.Parse( express[ 0 ] );
									y = float.Parse( express[ 1 ] );
								}
								catch ( Exception e ) {
									Console.WriteLine( function.Name );
									Console.WriteLine( parameter );
									Console.WriteLine( express[ 0 ] );
									Console.WriteLine( express[ 1 ] );
									Console.WriteLine( e );
									throw;
								}
							}
							before += $"OPTIONAL_IM_VEC_2_ARG( {parameter.Name}, {x}, {y} )\n";
						}
						else {
							before += $"IM_VEC_2_ARG( {parameter.Name} )\n";
						}
					}
					else if ( referenceType.GetDisplayName() == KStructImVec4 ||
					          referenceType.ElementType is CppQualifiedType && ((CppQualifiedType)referenceType.ElementType).ElementType.GetDisplayName() == KStructImVec4 ) {
						if ( parameter.InitValue != null || parameter.InitExpression != null ) {
							var express =
								( parameter.InitExpression as CppRawExpression ).Text
								.Replace( KStructImVec4, string.Empty )
								.Replace( "(", string.Empty )
								.Replace( ")", string.Empty )
								.Replace( "-FLT_MIN", float.MinValue.ToString() )
								.Replace( "FLT_MAX", float.MaxValue.ToString() )
								.Trim()
								.Split( ',' );
							float x = 0, y = 0, z = 0, w = 0;

							try {
								if ( express.Length > 0 ) {
									x = Convert.ToSingle( express[ 0 ] );
									y = Convert.ToSingle( express[ 1 ] );
									z = Convert.ToSingle( express[ 2 ] );
									w = Convert.ToSingle( express[ 3 ] );
								}
							}
							catch ( Exception e ) {
								Console.WriteLine( function.Name );
								Console.WriteLine( parameter );
								Console.WriteLine( express[ 0 ] );
								Console.WriteLine( express[ 1 ] );
								Console.WriteLine( express[ 2 ] );
								Console.WriteLine( express[ 3 ] );
								Console.WriteLine( e );
								throw;
							}
							
							before += $"OPTIONAL_IM_VEC_4_ARG( {parameter.Name}, {x}, {y}, {z}, {w} )\n";
						}
						else {
							before += $"IM_VEC_4_ARG( {parameter.Name} )\n";
						}
					}
					else {
						goto Unsupported;
					}
					break;
				case CppTypeKind.Enum:
					if ( KSupportedEnums.Contains( parameter.Type.GetDisplayName() ) ) {
						if ( parameter.InitValue != null ) {
							before += $"OPTIONAL_INT_ARG( {parameter.Name}, {parameter.InitValue.Value} )\n";
						}
						else {
							before += $"INT_ARG( {parameter.Name} )\n";
						}
					}
					else {
						goto Unsupported;
					}
					break;
				case CppTypeKind.Typedef:
					if ( KSupportedUIntTypedefs.Contains( parameter.Type.GetDisplayName() ) ) {
						if ( parameter.InitValue != null ) {
							// before += $"OPTIONAL_UINT_ARG( {parameter.Name}, ( {parameter.Type.GetDisplayName()} ){parameter.InitValue.Value} )\n";
							before += $"OPTIONAL_UINT_ARG( {parameter.Name}, {parameter.InitValue.Value} )\n";
						}
						else {
							// before += $"UINT_ARG( ({parameter.Type.GetDisplayName()}){parameter.Name} )\n";
							before += $"UINT_ARG( {parameter.Name} )\n";
						}
					}
					else if ( KSupportedTypedefs.Contains( parameter.Type.GetDisplayName() ) ) {
						if ( parameter.InitValue != null ) {
							// before += $"OPTIONAL_INT_ARG( {parameter.Name}, ( {parameter.Type.GetDisplayName()} ){parameter.InitValue.Value} )\n";
							before += $"OPTIONAL_INT_ARG( {parameter.Name}, {parameter.InitValue.Value} )\n";
						}
						else {
							// before += $"INT_ARG( ({parameter.Type.GetDisplayName()}){parameter.Name} )\n";
							before += $"INT_ARG( {parameter.Name} )\n";
						}
					}
					else if ( KSupportedTypedefCallbacks.Contains( parameter.Type.GetDisplayName() ) ) {
						if ( parameter.InitValue != null ) {
							before += $"CALLBACK_STUB( {parameter.Name}, {parameter.Type.GetDisplayName()} )\n";
						}
						else if ( parameter.InitExpression != null ) {
							// var rawExpression = parameter.InitExpression as CppRawExpression;
							// var exprValue = string.IsNullOrEmpty( rawExpression.Text ) ? "0" : rawExpression.Text;
							before += $"CALLBACK_STUB( {parameter.Name}, {parameter.Type.GetDisplayName()} )\n";
						}
						else {
							before += $"CALLBACK_STUB( {parameter.Name}, {parameter.Type.GetDisplayName()} )\n";
						}
					}
					else if ( parameter.Type.GetDisplayName() == KTypedefImTextureID ) {
						before += $"IM_TEXTURE_ID_ARG( {parameter.Name} )\n";
					}
					else {
						goto Unsupported;
					}
					break;
				case CppTypeKind.Array:
					var arrayType = parameter.Type as CppArrayType;
					if ( arrayType.ElementType.Equals( CppPrimitiveType.Int ) ) {
						before += $"INT_ARRAY_DEF( {parameter.Name}, {arrayType.Size} )\n";
						for ( int i = 0; i < arrayType.Size; i++ ) {
							before += $"INT_ARRAY_ARG( {parameter.Name}, {i} )\n";
							after += $"PUSH_NUMBER( {parameter.Name}[ {i} ] )\n";
						}
					}
					else if ( arrayType.ElementType.Equals( CppPrimitiveType.Float ) ) {
						before += $"FLOAT_ARRAY_DEF( {parameter.Name}, {arrayType.Size} )\n";
						for ( int i = 0; i < arrayType.Size; i++ ) {
							before += $"FLOAT_ARRAY_ARG( {parameter.Name}, {i} )\n";
							after += $"PUSH_NUMBER( {parameter.Name}[ {i} ] )\n";
						}
					}
					else if ( arrayType.ElementType.GetDisplayName() == KConstCharPointer ) {
						before += $"LABEL_ARRAY_TABLE_ARG( {parameter.Name} )\n";
					}
					else {
						goto Unsupported;
					}
					break;
				default:
					Unsupported:
					parameterSupported = false;
					supported = false;
#if LOG_TODO
					Console.WriteLine( $"TODO: unsupported CppParameter {parameter}" );
					Console.WriteLine( $"{function}" );
#endif
					break;
			}

			if ( parameterSupported ) {
				if ( parameter.Type.TypeKind == CppTypeKind.Typedef ||
				     parameter.Type.TypeKind == CppTypeKind.Enum ) {
					PushFuncArg( $"({parameter.Type.GetDisplayName()}){parameter.Name}" );
				}
				else {
					PushFuncArg( parameter.Name );
				}
			}
			
		}
		
		// TryWriteFunctionDef
		WriteFunctionReturn();
		foreach ( var param in function.Parameters ) {
			WriteFunctionParam( param );
		}

		// Output Result
		if ( supported ) {
			builder.Append( $"//----------------------------------------------------------------//\n" );
			builder.Append( $"// {function}\n" );
			builder.Append( $"//----------------------------------------------------------------//\n" );
			builder.Append( $"IMGUI_FUNCTION{functionSuffix}( {luaFunc} )\n" );
			builder.Append( $"{before}" );
			builder.Append( $"{callMacro}( {funcName}" );
			foreach ( var arg in funcArgs ) {
				builder.Append( $", {arg}" );
			}
			builder.Append( " )\n" );
			
			builder.Append( after );
			builder.AppendLine( "END_IMGUI_FUNC\n" );
		}
		else {
			builder.Append( $"//----------------------------------------------------------------//\n" );
			builder.AppendLine( $"// unsupported: {function}" );
			builder.AppendLine( $"//----------------------------------------------------------------//\n" );
		}

		return supported;
	}
	
}