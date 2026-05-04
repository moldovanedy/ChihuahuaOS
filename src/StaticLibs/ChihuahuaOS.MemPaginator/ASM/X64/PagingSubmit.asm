section .text

; UEFI calling convention

global Paging_SubmitPageTable
Paging_SubmitPageTable:
    mov cr3, rcx
    ret
    
global Paging_InvalidatePage
Paging_InvalidatePage:
    invlpg [rcx]
    ret