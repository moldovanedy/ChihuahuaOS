section .text

; UEFI calling convention

extern SpinLocks_HaltInfLoop

global SetupAndJumpToKernel_Call
SetupAndJumpToKernel_Call:
    ; set the root page table to the CR3 register
    mov cr3, rcx
    
    ; setup the kernel stack (this is the address of the top of the stack)
    mov rsp, 0xFFFFFFFFFFFF0000
    
    ;call SpinLocks_HaltInfLoop
    
    ; call the kernel (finally)
    jmp rdx

    ; unreachable (if we reach here, it's going to have a stack overflow since there is nowhere to return to, since
    ;  we just set up a new stack earlier)
    ret