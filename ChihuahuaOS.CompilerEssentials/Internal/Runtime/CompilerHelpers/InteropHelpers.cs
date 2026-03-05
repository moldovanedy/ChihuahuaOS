// bflat minimal runtime library
// Copyright (C) 2021-2022 Michal Strehovsky
//
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU Affero General Public License as published
// by the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
//
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU Affero General Public License for more details.
//
// You should have received a copy of the GNU Affero General Public License
// along with this program.  If not, see <https://www.gnu.org/licenses/>.

using System;
using System.Runtime.InteropServices;

namespace Internal.Runtime.CompilerHelpers;

public static unsafe class InteropHelpers
{
    private static IntPtr ResolvePInvoke(MethodFixupCell* pCell)
    {
        if (pCell->Target != 0)
            return pCell->Target;

        return ResolvePInvokeSlow(pCell);
    }

    private static IntPtr ResolvePInvokeSlow(MethodFixupCell* pCell)
    {
        ModuleFixupCell* pModuleCell = pCell->Module;
        if (pModuleCell->Handle == 0)
        {
            if (pModuleCell->Handle == 0)
                Environment.FailFast(null!);
        }

        if (pCell->Target == 0)
            Environment.FailFast(null!);

        return pCell->Target;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct ModuleFixupCell
    {
        public IntPtr Handle;
        public IntPtr ModuleName;
        public IntPtr CallingAssemblyType;
        public uint DllImportSearchPathAndCookie;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct MethodFixupCell
    {
        public IntPtr Target;
        public IntPtr MethodName;
        public ModuleFixupCell* Module;
        private int Flags;
    }
}