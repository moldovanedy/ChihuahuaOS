OUTPUT_FORMAT(elf64-x86-64)
ENTRY(_start)

SECTIONS {
    /* load at a high virtual address (higher-half kernel) */
    . = 0xFFFFEEEE80000000;

    .text : ALIGN(4K)
	{
		*(.text __managedcode)
	}

    /* Read-only data. */
	.rodata : ALIGN(4K)
	{
		*(.rodata .dotnet_eh_table .eh_frame)
	}

	/* Read-write data (initialized) */
	.data : ALIGN(4K)
	{
		*(.data)
	}

    .bss : ALIGN(4K)
	{
		*(COMMON)
		*(.bss)
	}
}
