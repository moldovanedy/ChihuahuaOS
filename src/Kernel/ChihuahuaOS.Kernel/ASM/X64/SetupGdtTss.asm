section .data

TSS_SIZE equ 104
KERNEL_CS_OFFSET equ 0x08
KERNEL_DS_OFFSET equ 0x10
TSS_OFFSET equ 0x28

gdt_start:
    dq 0                    ; null

    dq 0x00AF9A000000FFFF   ; kernel code
    dq 0x00CF92000000FFFF   ; kernel data
    
    dq 0x00CFF2000000FFFF   ; user code
    dq 0x00AFFA000000FFFF   ; user data

    ; TSS Descriptor (16 bytes in x64)
    dw TSS_SIZE - 1    ; Limit (low)
    dw 0               ; Base (low 16 bits)
    db 0               ; Base (mid 8 bits)
    db 0x89            ; Type (TSS, available), P=1, DPL=0
    db 0x20            ; Limit (high 4 bits) + flags
    db 0               ; Base (high 8 bits)
    dd 0               ; Base (upper 32 bits)
    dd 0               ; Reserved
gdt_end:


tss_start:
    times 102 db 0
    dw TSS_SIZE
tss_end:


section .text

gdt_descriptor:
    dw gdt_end - gdt_start - 1
    dq gdt_start


global Kernel_SetupGdtAndTss
Kernel_SetupGdtAndTss:
    ; write TSS base address in GDT
    lea rax, [rel tss_start]
    mov [rel gdt_start + TSS_OFFSET + 2], ax
    shr rax, 16
    mov [rel gdt_start + TSS_OFFSET + 4], al
    shr rax, 8
    mov [rel gdt_start + TSS_OFFSET + 7], al
    shr rax, 8
    mov [rel gdt_start + TSS_OFFSET + 8], eax

    lgdt [rel gdt_descriptor]

    ; Reload CS
    push KERNEL_CS_OFFSET
    lea rax, [rel .reload_cs]
    push rax
    retfq


.reload_cs:
    ; Reload data segments
    mov ax, KERNEL_DS_OFFSET
    mov ds, ax
    mov es, ax
    mov fs, ax
    mov gs, ax
    mov ss, ax

    ; Load Task Register
    mov ax, TSS_OFFSET
    ltr ax

    ret