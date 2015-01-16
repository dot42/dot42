using Dot42.CompilerLib.Ast;
using Mono.Cecil;
using Mono.Cecil.Cil;

namespace Dot42.CompilerLib.IL2Ast
{
    public static class AstCodeUtil
	{
		public static void ExpandMacro(ref AstCode code, ref object operand, MethodBody methodBody)
		{
			switch (code) {
					case AstCode.__Ldarg_0:   code = AstCode.__Ldarg; operand = methodBody.GetParameter(0); break;
					case AstCode.__Ldarg_1:   code = AstCode.__Ldarg; operand = methodBody.GetParameter(1); break;
					case AstCode.__Ldarg_2:   code = AstCode.__Ldarg; operand = methodBody.GetParameter(2); break;
					case AstCode.__Ldarg_3:   code = AstCode.__Ldarg; operand = methodBody.GetParameter(3); break;
					case AstCode.__Ldloc_0:   code = AstCode.Ldloc; operand = methodBody.Variables[0]; break;
					case AstCode.__Ldloc_1:   code = AstCode.Ldloc; operand = methodBody.Variables[1]; break;
					case AstCode.__Ldloc_2:   code = AstCode.Ldloc; operand = methodBody.Variables[2]; break;
					case AstCode.__Ldloc_3:   code = AstCode.Ldloc; operand = methodBody.Variables[3]; break;
					case AstCode.__Stloc_0:   code = AstCode.Stloc; operand = methodBody.Variables[0]; break;
					case AstCode.__Stloc_1:   code = AstCode.Stloc; operand = methodBody.Variables[1]; break;
					case AstCode.__Stloc_2:   code = AstCode.Stloc; operand = methodBody.Variables[2]; break;
					case AstCode.__Stloc_3:   code = AstCode.Stloc; operand = methodBody.Variables[3]; break;
					case AstCode.__Ldarg_S:   code = AstCode.__Ldarg; break;
					case AstCode.__Ldarga_S:  code = AstCode.__Ldarga; break;
					case AstCode.__Starg_S:   code = AstCode.__Starg; break;
					case AstCode.__Ldloc_S:   code = AstCode.Ldloc; break;
					case AstCode.__Ldloca_S:  code = AstCode.Ldloca; break;
					case AstCode.__Stloc_S:   code = AstCode.Stloc; break;
					case AstCode.__Ldc_I4_M1: code = AstCode.Ldc_I4; operand = -1; break;
					case AstCode.__Ldc_I4_0:  code = AstCode.Ldc_I4; operand = 0; break;
					case AstCode.__Ldc_I4_1:  code = AstCode.Ldc_I4; operand = 1; break;
					case AstCode.__Ldc_I4_2:  code = AstCode.Ldc_I4; operand = 2; break;
					case AstCode.__Ldc_I4_3:  code = AstCode.Ldc_I4; operand = 3; break;
					case AstCode.__Ldc_I4_4:  code = AstCode.Ldc_I4; operand = 4; break;
					case AstCode.__Ldc_I4_5:  code = AstCode.Ldc_I4; operand = 5; break;
					case AstCode.__Ldc_I4_6:  code = AstCode.Ldc_I4; operand = 6; break;
					case AstCode.__Ldc_I4_7:  code = AstCode.Ldc_I4; operand = 7; break;
					case AstCode.__Ldc_I4_8:  code = AstCode.Ldc_I4; operand = 8; break;
					case AstCode.__Ldc_I4_S:  code = AstCode.Ldc_I4; operand = (int) (sbyte) operand; break;
					case AstCode.__Br_S:      code = AstCode.Br; break;
					case AstCode.__Brfalse_S: code = AstCode.Brfalse; break;
					case AstCode.__Brtrue_S:  code = AstCode.Brtrue; break;
					case AstCode.__Beq_S:     code = AstCode.__Beq; break;
					case AstCode.__Bge_S:     code = AstCode.__Bge; break;
					case AstCode.__Bgt_S:     code = AstCode.__Bgt; break;
					case AstCode.__Ble_S:     code = AstCode.__Ble; break;
					case AstCode.__Blt_S:     code = AstCode.__Blt; break;
					case AstCode.__Bne_Un_S:  code = AstCode.__Bne_Un; break;
					case AstCode.__Bge_Un_S:  code = AstCode.__Bge_Un; break;
					case AstCode.__Bgt_Un_S:  code = AstCode.__Bgt_Un; break;
					case AstCode.__Ble_Un_S:  code = AstCode.__Ble_Un; break;
					case AstCode.__Blt_Un_S:  code = AstCode.__Blt_Un; break;
					case AstCode.__Leave_S:   code = AstCode.Leave; break;
					case AstCode.__Ldind_I:   code = AstCode.Ldobj; operand = methodBody.Method.Module.TypeSystem.IntPtr; break;
					case AstCode.__Ldind_I1:  code = AstCode.Ldobj; operand = methodBody.Method.Module.TypeSystem.SByte; break;
					case AstCode.__Ldind_I2:  code = AstCode.Ldobj; operand = methodBody.Method.Module.TypeSystem.Int16; break;
					case AstCode.__Ldind_I4:  code = AstCode.Ldobj; operand = methodBody.Method.Module.TypeSystem.Int32; break;
					case AstCode.__Ldind_I8:  code = AstCode.Ldobj; operand = methodBody.Method.Module.TypeSystem.Int64; break;
					case AstCode.__Ldind_U1:  code = AstCode.Ldobj; operand = methodBody.Method.Module.TypeSystem.Byte; break;
					case AstCode.__Ldind_U2:  code = AstCode.Ldobj; operand = methodBody.Method.Module.TypeSystem.UInt16; break;
					case AstCode.__Ldind_U4:  code = AstCode.Ldobj; operand = methodBody.Method.Module.TypeSystem.UInt32; break;
					case AstCode.__Ldind_R4:  code = AstCode.Ldobj; operand = methodBody.Method.Module.TypeSystem.Single; break;
					case AstCode.__Ldind_R8:  code = AstCode.Ldobj; operand = methodBody.Method.Module.TypeSystem.Double; break;
					case AstCode.__Stind_I:   code = AstCode.Stobj; operand = methodBody.Method.Module.TypeSystem.IntPtr; break;
					case AstCode.__Stind_I1:  code = AstCode.Stobj; operand = methodBody.Method.Module.TypeSystem.Byte; break;
					case AstCode.__Stind_I2:  code = AstCode.Stobj; operand = methodBody.Method.Module.TypeSystem.Int16; break;
					case AstCode.__Stind_I4:  code = AstCode.Stobj; operand = methodBody.Method.Module.TypeSystem.Int32; break;
					case AstCode.__Stind_I8:  code = AstCode.Stobj; operand = methodBody.Method.Module.TypeSystem.Int64; break;
					case AstCode.__Stind_R4:  code = AstCode.Stobj; operand = methodBody.Method.Module.TypeSystem.Single; break;
					case AstCode.__Stind_R8:  code = AstCode.Stobj; operand = methodBody.Method.Module.TypeSystem.Double; break;
			}
		}
		
		public static ParameterDefinition GetParameter (this MethodBody self, int index)
		{
			var method = self.Method;

			if (method.HasThis) {
				if (index == 0)
					return self.ThisParameter;

				index--;
			}

			var parameters = method.Parameters;

			if (index < 0 || index >= parameters.Count)
				return null;

			return parameters [index];
		}
	}
}
