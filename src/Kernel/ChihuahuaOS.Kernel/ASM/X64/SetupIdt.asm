section .data

KERNEL_CS_OFFSET equ 0x08

; IDT entry structure (x86_64)
; 0-1: Offset low
; 2-3: Selector
; 4: IST
; 5: Type/Attributes
; 6-7: Offset mid
; 8-11: Offset high
; 12-15: Reserved

align 16
idt_start:
    times 14 * 16 db 0 ; Skip first 14 entries

    ; Page Fault (Vector 14)
    dw 0
    dw KERNEL_CS_OFFSET
    db 0 ; IST
    db 0x8E ; Type: Interrupt Gate, P=1, DPL=0
    dw 0
    dd 0
    dd 0 ; Reserved

    times (256 - 15) * 16 db 0 ; Skip remaining entries
idt_end:

idt_descriptor:
    dw idt_end - idt_start - 1
    dq idt_start
    

section .text

global Kernel_SetupIdt
Kernel_SetupIdt:
    
    ; Vector 14: Page fault
    lea rax, [rel PageFaultHandler]
    lea rdi, [rel idt_start + 14 * 16]
    
    mov [rdi], ax ; Offset low
    shr rax, 16
    mov [rdi + 6], ax ; Offset mid
    shr rax, 16
    mov [rdi + 8], eax ; Offset high

    lidt [rel idt_descriptor]
    ret

; Standard x86_64 Exception Stack Frame (with error code):
; [SS]
; [RSP]
; [RFLAGS]
; [CS]
; [RIP]
; [Error Code] <- RSP points here on entry
    
PageFaultHandler:
    mov rax, 0xBA5E
    .loop:
        hlt
        jmp .loop
        
