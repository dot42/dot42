using System;
using System.Collections.Generic;
using System.Linq;
using Dot42.CompilerLib.XModel;

namespace Dot42.CompilerLib.Ast.Optimizer
{
	/// <summary>
	/// IL AST transformation that introduces array, object and collection initializers.
	/// </summary>
	public partial class AstOptimizer
	{
		bool TransformArrayInitializers(List<AstNode> body, AstExpression expr, int pos)
		{
			AstVariable v, v3;
			AstExpression newarrExpr;
			XTypeReference elementType;
			AstExpression lengthExpr;
			int arrayLength;
			if (expr.Match(AstCode.Stloc, out v, out newarrExpr) &&
			    newarrExpr.Match(AstCode.Newarr, out elementType, out lengthExpr) &&
			    lengthExpr.Match(AstCode.Ldc_I4, out arrayLength) &&
			    arrayLength > 0) {
				Array newArr;
				int initArrayPos;
                if (ForwardScanInitializeArrayRuntimeHelper(body, pos + 1, v, elementType, arrayLength, out newArr, out initArrayPos))
                {
                    var arrayType = new XArrayType(elementType, new XArrayDimension(0, arrayLength));
                    body[pos] = new AstExpression(expr.SourceLocation, AstCode.Stloc, v, new AstExpression(expr.SourceLocation, AstCode.InitArray, 
                        new InitArrayData(arrayType, newArr)));
                    body.RemoveAt(initArrayPos);
                }
#if !NOTUSED 
                // Put in a limit so that we don't consume too much memory if the code allocates a huge array
				// and populates it extremely sparsly. However, 255 "null" elements in a row actually occur in the Mono C# compiler!
			        const int maxConsecutiveDefaultValueExpressions = 0;// 300;
				var operands = new List<AstExpression>();
				int numberOfInstructionsToRemove = 0;
				for (int j = pos + 1; j < body.Count; j++) {
					var nextExpr = body[j] as AstExpression;
					int arrayPos;
					if (nextExpr != null &&
					    nextExpr.Code.IsStoreToArray() &&
					    nextExpr.Arguments[0].Match(AstCode.Ldloc, out v3) &&
					    v == v3 &&
					    nextExpr.Arguments[1].Match(AstCode.Ldc_I4, out arrayPos) &&
					    arrayPos >= operands.Count &&
					    arrayPos <= operands.Count + maxConsecutiveDefaultValueExpressions) {
						while (operands.Count < arrayPos)
							operands.Add(new AstExpression(expr.SourceLocation, AstCode.DefaultValue, elementType));
						operands.Add(nextExpr.Arguments[2]);
						numberOfInstructionsToRemove++;
					} else {
						break;
					}
				}
				/*if (operands.Count == arrayLength) {
					var arrayType = new XArrayType(elementType, new XArrayDimension(0, arrayLength));
					expr.Arguments[0] = new AstExpression(expr.SourceLocation, AstCode.InitArray, arrayType, operands);
					body.RemoveRange(pos + 1, numberOfInstructionsToRemove);

					new AstInlining(method).InlineIfPossible(body, ref pos);
					return true;
				}*/
#endif
			}
			return false;
		}

	    private bool TransformMultidimensionalArrayInitializers(List<AstNode> body, AstExpression expr, int pos)
	    {
	        AstVariable v;
	        AstExpression newarrExpr;
	        XMethodReference ctor;
	        List<AstExpression> ctorArgs;
	        XArrayType arrayType;
	        if (expr.Match(AstCode.Stloc, out v, out newarrExpr) &&
	            newarrExpr.Match(AstCode.Newobj, out ctor, out ctorArgs) &&
	            (arrayType = (ctor.DeclaringType as XArrayType)) != null &&
	            arrayType.Rank == ctorArgs.Count)
	        {
	            // Clone the type, so we can muck about with the Dimensions
	            var arrayLengths = new int[arrayType.Rank];
	            var dimensions = new XArrayDimension[arrayType.Rank];
	            for (int i = 0; i < arrayType.Rank; i++)
	            {
	                if (!ctorArgs[i].Match(AstCode.Ldc_I4, out arrayLengths[i])) return false;
	                if (arrayLengths[i] <= 0) return false;
	                dimensions[i] = new XArrayDimension(0, arrayLengths[i]);
	            }
                arrayType = new XArrayType(arrayType.ElementType, dimensions);

	            var totalElements = arrayLengths.Aggregate(1, (t, l) => t*l);
	            Array newArr;
	            int initArrayPos;
	            if (ForwardScanInitializeArrayRuntimeHelper(body, pos + 1, v, arrayType, totalElements, out newArr,
	                                                        out initArrayPos))
	            {
	                body[pos] = new AstExpression(expr.SourceLocation, AstCode.Stloc, v,
	                                              new AstExpression(expr.SourceLocation, AstCode.InitArray,
	                                                                new InitArrayData(arrayType, newArr)));
	                body.RemoveAt(initArrayPos);
	                return true;
	            }
	        }
	        return false;
	    }

	    bool ForwardScanInitializeArrayRuntimeHelper(List<AstNode> body, int pos, AstVariable array, XTypeReference arrayType, int arrayLength, out Array values, out int foundPos)
		{
			AstVariable v2;
			XMethodReference methodRef;
			AstExpression methodArg1;
			AstExpression methodArg2;
			XFieldReference fieldRef;
			if (body.ElementAtOrDefault(pos).Match(AstCode.Call, out methodRef, out methodArg1, out methodArg2) &&
			    methodRef.DeclaringType.FullName == "System.Runtime.CompilerServices.RuntimeHelpers" &&
			    methodRef.Name == "InitializeArray" &&
			    methodArg1.Match(AstCode.Ldloc, out v2) &&
			    array == v2 &&
			    methodArg2.Match(AstCode.Ldtoken, out fieldRef))
			{
				var fieldDef = fieldRef.Resolve(); //.ResolveWithinSameModule();
				if (fieldDef != null && fieldDef.InitialValue != null)
				{
				    Array newArr;
					if (DecodeArrayInitializer(arrayType.GetElementType(), (byte[]) fieldDef.InitialValue, arrayLength, out newArr))
					{
						values = newArr;
						foundPos = pos;
						return true;
					}
				}
			}
			values = null;
			foundPos = -1;
			return false;
		}

		static bool DecodeArrayInitializer(XTypeReference elementTypeRef, byte[] initialValue, int arrayLength, out Array output)
		{
			var elementType = TypeAnalysis.GetTypeCode(elementTypeRef);
			switch (elementType) {
				case TypeCode.Boolean:
			        output = new bool[arrayLength];
                    return DecodeArrayInitializer(initialValue, (bool[])output, elementType, (d, i) => (d[i] != 0));
                case TypeCode.Byte:
			        output = new byte[arrayLength];
					return DecodeArrayInitializer(initialValue, (byte[])output, elementType, (d, i) => (byte)d[i]);
				case TypeCode.SByte:
			        output = new sbyte[arrayLength];
					return DecodeArrayInitializer(initialValue, (sbyte[])output, elementType, (d, i) => (sbyte)d[i]);
				case TypeCode.Int16:
			        output = new short[arrayLength];
					return DecodeArrayInitializer(initialValue, (short[])output, elementType, BitConverter.ToInt16);
				case TypeCode.Char:
			        output = new char[arrayLength];
                    return DecodeArrayInitializer(initialValue, (char[])output, elementType, BitConverter.ToChar);
                case TypeCode.UInt16:
			        output = new ushort[arrayLength];
					return DecodeArrayInitializer(initialValue, (ushort[])output, elementType, BitConverter.ToUInt16);
				case TypeCode.Int32:
			        output = new int[arrayLength];
                    return DecodeArrayInitializer(initialValue, (int[])output, elementType, BitConverter.ToInt32);
                case TypeCode.UInt32:
			        output = new uint[arrayLength];
					return DecodeArrayInitializer(initialValue, (uint[])output, elementType, BitConverter.ToUInt32);
				case TypeCode.Int64:
			        output = new long[arrayLength];
                    return DecodeArrayInitializer(initialValue, (long[])output, elementType, BitConverter.ToInt64);
                case TypeCode.UInt64:
			        output = new ulong[arrayLength];
					return DecodeArrayInitializer(initialValue, (ulong[])output, elementType, BitConverter.ToUInt64);
				case TypeCode.Single:
			        output = new float[arrayLength];
					return DecodeArrayInitializer(initialValue, (float[])output, elementType, BitConverter.ToSingle);
				case TypeCode.Double:
			        output = new double[arrayLength];
					return DecodeArrayInitializer(initialValue, (double[])output, elementType, BitConverter.ToDouble);
				case TypeCode.Object:
					var typeDef = elementTypeRef.Resolve(); //.ResolveWithinSameModule();
					if (typeDef != null && typeDef.IsEnum)
						return DecodeArrayInitializer(typeDef.GetEnumUnderlyingType(), initialValue, arrayLength, out output);
			        output = null;
					return false;
				default:
			        output = null;
					return false;
			}
		}

		static bool DecodeArrayInitializer<T>(byte[] initialValue, T[] output, TypeCode elementType, Func<byte[], int, T> decoder)
		{
			int elementSize = ElementSizeOf(elementType);
			if (initialValue.Length < (output.Length * elementSize))
				return false;

            for (int i = 0; i < output.Length; i++)
            {
                output[i] = decoder(initialValue, i*elementSize);
            }

		    return true;
		}

		private static AstCode LoadCodeFor(TypeCode elementType)
		{
			switch (elementType) {
				case TypeCode.Boolean:
				case TypeCode.Byte:
				case TypeCode.SByte:
				case TypeCode.Char:
				case TypeCode.Int16:
				case TypeCode.UInt16:
				case TypeCode.Int32:
				case TypeCode.UInt32:
					return AstCode.Ldc_I4;
				case TypeCode.Int64:
				case TypeCode.UInt64:
					return AstCode.Ldc_I8;
				case TypeCode.Single:
					return AstCode.Ldc_R4;
				case TypeCode.Double:
					return AstCode.Ldc_R8;
				default:
					throw new ArgumentOutOfRangeException("elementType");					
			}
		}

		private static int ElementSizeOf(TypeCode elementType)
		{
			switch (elementType) {
				case TypeCode.Boolean:
				case TypeCode.Byte:
				case TypeCode.SByte:
					return 1;
				case TypeCode.Char:
				case TypeCode.Int16:
				case TypeCode.UInt16:
					return 2;
				case TypeCode.Int32:
				case TypeCode.UInt32:
				case TypeCode.Single:
					return 4;
				case TypeCode.Int64:
				case TypeCode.UInt64:
				case TypeCode.Double:
					return 8;
				default:
					throw new ArgumentOutOfRangeException("elementType");
			}
		}
	}
}
