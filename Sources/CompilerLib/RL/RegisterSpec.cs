using System;
using System.Collections.Generic;
using System.Diagnostics;
using Dot42.DexLib;

namespace Dot42.CompilerLib.RL
{
    /// <summary>
    /// Register indicator + type contained in the register
    /// </summary>
    [DebuggerDisplay("{@Register}")]
    public class RegisterSpec
    {
        private readonly Register register;
        private readonly Register wideRegister;
        private readonly TypeReference type;

        /// <summary>
        /// Default ctor
        /// </summary>
        internal RegisterSpec(Register register, Register wideRegister, TypeReference type)
        {
            if (register == null)
                throw new ArgumentNullException("register");
            if (type == null)
                throw new ArgumentNullException("type");
            this.register = register;
            this.wideRegister = wideRegister;
            this.type = type;
        }

        public TypeReference Type
        {
            get { return type; }
        }

        /// <summary>
        /// Is this a 64-bit register usage?
        /// </summary>
        public bool IsWide { get { return (wideRegister != null); } }

        /// <summary>
        /// Gets the number of registers used by me.
        /// </summary>
        public int RegisterCount { get { return IsWide ? 2 : 1; } }

        /// <summary>
        /// Gets the register.
        /// </summary>
        public Register Register { get { return register; } }

        /// <summary>
        /// Gets the second register.
        /// </summary>
        public Register Register2 { get { return wideRegister; } }

        /// <summary>
        /// Gets the registers (1 for normal, 2 for wide).
        /// </summary>
        public IEnumerable<Register> Registers
        {
            get
            {
                yield return register;
                if (IsWide)
                    yield return wideRegister;
            }
        }

        /// <summary>
        /// Does this set contain the given register?
        /// </summary>
        public bool Contains(Register r)
        {
            return (r != null) && ((register == r) || (wideRegister == r));
        }

        /// <summary>
        /// Convert register spec to register.
        /// </summary>
        public static implicit operator Register(RegisterSpec registerSpec)
        {
            return registerSpec.register;
        }
    }
}
