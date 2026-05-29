section .text

; UEFI calling convention

global Paging_SubmitPageTable__UefiAbi
Paging_SubmitPageTable__UefiAbi:
    mov cr3, rcx
    ret

global Paging_InvalidatePage__UefiAbi
Paging_InvalidatePage__UefiAbi:
    invlpg [rcx]
    ret

global Paging_SubmitPageTable__SysVAbi
Paging_SubmitPageTable__SysVAbi:
    mov cr3, rdi
    ret

global Paging_InvalidatePage__SysVAbi
Paging_InvalidatePage__SysVAbi:
    invlpg [rdi]
    ret
    

global Paging_GetRootPageTable
Paging_GetRootPageTable:
    mov rax, cr3
    ret