section .text

; UEFI calling convention

global SetupAndJumpToKernel_Call
SetupAndJumpToKernel_Call:
    ; set the root page table to the CR3 register
    mov cr3, rcx
    
    ; setup the kernel stack (this is the address of the top of the stack, the 4'th argument)
    mov rsp, r9
    
    ; move the kernel params address from the third argument of this function (UEFI)
    ;  to the first argument of the kernel (SysV)
    mov rdi, r8

    ; call the kernel (finally)
    jmp rdx

    ; unreachable
    ret