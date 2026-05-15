section .text

global SpinLocks_HaltInfLoop
SpinLocks_HaltInfLoop:
    loop:
        cli
        hlt
        jmp loop