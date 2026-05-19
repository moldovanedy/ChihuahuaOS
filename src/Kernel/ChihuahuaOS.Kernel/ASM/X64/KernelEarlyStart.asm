section .text

extern KInit
extern Kernel_SetupGdtAndTss
extern Kernel_SetupIdt

global _start
_start:
    ; preserve kernel params pointer
    mov r15, rdi
    
    call Kernel_SetupGdtAndTss
    call Kernel_SetupIdt

    ; restore pointer
    mov rdi, r15
    call KInit

    ; if KInit returns (it shouldn't), halt
.halt:
    cli
    hlt
    jmp .halt