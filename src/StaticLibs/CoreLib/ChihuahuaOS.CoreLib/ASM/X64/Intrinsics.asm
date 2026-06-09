section .text

global Intrinsics_ReadTimestamp
Intrinsics_ReadTimestamp:
    rdtsc
    mov r8, rdx
    shl r8, 32
    or rax, r8
    ret
