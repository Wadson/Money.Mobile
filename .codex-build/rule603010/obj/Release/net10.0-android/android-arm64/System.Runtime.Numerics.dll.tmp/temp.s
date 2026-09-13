.section ".debug_abbrev"
.subsection 0
.Ldebug_abbrev_start:

	.byte 1,17,1,37,8,3,8,27,8,19,11,17,1,18,1,16,6,0,0,2,46,1,3,8,135,64,8,58,15,59,15,90
	.byte 8,17,1,18,1,64,10,0,0,3,5,0,3,8,73,19,2,10,0,0,15,5,0,3,8,73,19,2,6,0,0,4
	.byte 36,0,11,11,62,11,3,8,0,0,5,2,1,3,8,11,15,0,0,17,2,0,3,8,11,15,0,0,6,13,0,3
	.byte 8,73,19,56,10,0,0,7,22,0,3,8,73,19,0,0,8,4,1,3,8,11,15,73,19,0,0,9,40,0,3,8
	.byte 28,13,0,0,10,57,1,3,8,0,0,11,52,0,3,8,73,19,2,10,0,0,12,52,0,3,8,73,19,2,6,0
	.byte 0,13,15,0,73,19,0,0,14,16,0,73,19,0,0,16,28,0,73,19,56,10,0,0,18,46,0,3,8,17,1,18
	.byte 1,0,0,0
.section ".debug_info"
.subsection 0
.Ldebug_info_start:

.LDIFF_SYM0=.Ldebug_info_end - .Ldebug_info_begin
	.long .LDIFF_SYM0
.Ldebug_info_begin:

	.short 2
	.long .Ldebug_abbrev_start
	.byte 8,1
	.string "Mono AOT Compiler 10.0.11.0 (10.0.11 @Commit: e2f47b0110ed922f21a1522da67279133ce28f32)"
	.string "System.Runtime.Numerics.dll"
	.string ""

	.byte 2,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0
.LDIFF_SYM1=.Ldebug_line_start - .Ldebug_line_section_start
	.long .LDIFF_SYM1
.LDIE_I1:

	.byte 4,1,5
	.string "sbyte"
.LDIE_U1:

	.byte 4,1,7
	.string "byte"
.LDIE_I2:

	.byte 4,2,5
	.string "short"
.LDIE_U2:

	.byte 4,2,7
	.string "ushort"
.LDIE_I4:

	.byte 4,4,5
	.string "int"
.LDIE_U4:

	.byte 4,4,7
	.string "uint"
.LDIE_I8:

	.byte 4,8,5
	.string "long"
.LDIE_U8:

	.byte 4,8,7
	.string "ulong"
.LDIE_I:

	.byte 4,8,5
	.string "intptr"
.LDIE_U:

	.byte 4,8,7
	.string "uintptr"
.LDIE_R4:

	.byte 4,4,4
	.string "float"
.LDIE_R8:

	.byte 4,8,4
	.string "double"
.LDIE_BOOLEAN:

	.byte 4,1,2
	.string "boolean"
.LDIE_CHAR:

	.byte 4,2,8
	.string "char"
.LDIE_STRING:

	.byte 4,8,1
	.string "string"
.LDIE_OBJECT:

	.byte 4,8,1
	.string "object"
.LDIE_SZARRAY:

	.byte 4,8,1
	.string "object"
.section ".debug_loc"
.subsection 0
.Ldebug_loc_start:
.section ".debug_frame"
.subsection 0
	.balign 8

.LDIFF_SYM2=.Lcie0_end - .Lcie0_start
	.long .LDIFF_SYM2
.Lcie0_start:

	.long -1
	.byte 3
	.string ""

	.byte 1,120,30
	.balign 8
.Lcie0_end:
.text 0
	.balign 8
jit_code_start:

	.byte 0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0
.text 0
	.balign 16
.Lm_78:
	.local System_Collections_Generic_ValueListBuilder_1_T_REF__ctor_System_Span_1_T_REF
	.type System_Collections_Generic_ValueListBuilder_1_T_REF__ctor_System_Span_1_T_REF,@function
System_Collections_Generic_ValueListBuilder_1_T_REF__ctor_System_Span_1_T_REF:
.inst 0xa9bd7bfd
.inst 0x910003fd
.inst 0xf9000bba
.inst 0xf90017af
.inst 0xaa0003fa
.inst 0xa9018ba1

adrp x16, mono_aot_System_Runtime_Numerics_got+0
add x16, x16, :lo12:mono_aot_System_Runtime_Numerics_got
ldr x0, [x16, 56]
.inst 0xf9400011
.inst 0xb4000051
bl .Lp_1
.inst 0xf9000b5f
.inst 0xb9001b5f
.inst 0xeb1f035f
.inst 0x10000011
.inst 0x54000120
.inst 0xf9400fa0
.inst 0xf9000340
.inst 0xf94013a0
.inst 0xf9000740
.inst 0xf9400bba
.inst 0x910003bf
.inst 0xa8c37bfd
.inst 0xd65f03c0
.inst 0xd2801e40
.inst 0xaa1103e1
bl .Lp_2

	.size System_Collections_Generic_ValueListBuilder_1_T_REF__ctor_System_Span_1_T_REF,.-System_Collections_Generic_ValueListBuilder_1_T_REF__ctor_System_Span_1_T_REF
.Lme_78:
.text 0
	.balign 16
.Lm_79:
	.local System_Collections_Generic_ValueListBuilder_1_T_REF_get_Length
	.type System_Collections_Generic_ValueListBuilder_1_T_REF_get_Length,@function
System_Collections_Generic_ValueListBuilder_1_T_REF_get_Length:
.inst 0xa9be7bfd
.inst 0x910003fd
.inst 0xf9000faf
.inst 0xf9000ba0

adrp x16, mono_aot_System_Runtime_Numerics_got+0
add x16, x16, :lo12:mono_aot_System_Runtime_Numerics_got
ldr x0, [x16, 56]
.inst 0xf9400011
.inst 0xb4000051
bl .Lp_1
.inst 0xf9400ba0
.inst 0xb9801800
.inst 0x910003bf
.inst 0xa8c27bfd
.inst 0xd65f03c0

	.size System_Collections_Generic_ValueListBuilder_1_T_REF_get_Length,.-System_Collections_Generic_ValueListBuilder_1_T_REF_get_Length
.Lme_79:
.text 0
	.balign 16
.Lm_7a:
	.local System_Collections_Generic_ValueListBuilder_1_T_REF_Append_T_REF
	.type System_Collections_Generic_ValueListBuilder_1_T_REF_Append_T_REF,@function
System_Collections_Generic_ValueListBuilder_1_T_REF_Append_T_REF:
.inst 0xa9bc7bfd
.inst 0x910003fd
.inst 0xa90167b8
.inst 0xf90017af
.inst 0xaa0003f9
.inst 0xf90013a1

adrp x16, mono_aot_System_Runtime_Numerics_got+0
add x16, x16, :lo12:mono_aot_System_Runtime_Numerics_got
ldr x0, [x16, 56]
.inst 0xf9400011
.inst 0xb4000051
bl .Lp_1

adrp x16, mono_aot_System_Runtime_Numerics_got+0
add x16, x16, :lo12:mono_aot_System_Runtime_Numerics_got
ldr x1, [x16, 200]
.inst 0xf94017a0
.inst 0xf9400c10
.inst 0xb5000050
bl .Lp_3
.inst 0xd2800000
.inst 0xf9001ba0
.inst 0xf9001fa0
.inst 0xb9801b38
.inst 0xf9400320
.inst 0xf9001ba0
.inst 0xf9400720
.inst 0xf9001fa0
.inst 0xb9803ba0
.inst 0x6b00031f
.inst 0x540002e2
.inst 0xf9401ba0
.inst 0x93407f01
.inst 0xb9803ba2
.inst 0xeb01005f
.inst 0x10000011
.inst 0x54000349
.inst 0xd37df021
.inst 0x8b010001
.inst 0xd5033bbf
.inst 0xf94013a0
.inst 0xf9000020
.inst 0xd349fc21
.inst 0x92405821

adrp x16, mono_aot_System_Runtime_Numerics_got+0
add x16, x16, :lo12:mono_aot_System_Runtime_Numerics_got
ldr x2, [x16, 16]
.inst 0x8b020021
.inst 0xd280003e
.inst 0x3900003e
.inst 0x11000700
.inst 0xb9001b20
.inst 0x14000006
.inst 0xf94017a0
.inst 0xf940100f
.inst 0xaa1903e0
.inst 0xf94013a1
bl .Lp_4
.inst 0xa94167b8
.inst 0x910003bf
.inst 0xa8c47bfd
.inst 0xd65f03c0
.inst 0xd2801960
.inst 0xaa1103e1
bl .Lp_2

	.size System_Collections_Generic_ValueListBuilder_1_T_REF_Append_T_REF,.-System_Collections_Generic_ValueListBuilder_1_T_REF_Append_T_REF
.Lme_7a:
.text 0
	.balign 16
.Lm_7b:
	.local System_Collections_Generic_ValueListBuilder_1_T_REF_Append_System_ReadOnlySpan_1_T_REF
	.type System_Collections_Generic_ValueListBuilder_1_T_REF_Append_System_ReadOnlySpan_1_T_REF,@function
System_Collections_Generic_ValueListBuilder_1_T_REF_Append_System_ReadOnlySpan_1_T_REF:
.inst 0xa9b97bfd
.inst 0x910003fd
.inst 0xa9016bb9
.inst 0xf90023af
.inst 0xaa0003fa
.inst 0xa9020ba1

adrp x16, mono_aot_System_Runtime_Numerics_got+0
add x16, x16, :lo12:mono_aot_System_Runtime_Numerics_got
ldr x0, [x16, 56]
.inst 0xf9400011
.inst 0xb4000051
bl .Lp_1

adrp x16, mono_aot_System_Runtime_Numerics_got+0
add x16, x16, :lo12:mono_aot_System_Runtime_Numerics_got
ldr x1, [x16, 208]
.inst 0xf94023a0
.inst 0xf9400c10
.inst 0xb5000050
bl .Lp_3
.inst 0xd2800000
.inst 0xf90027a0
.inst 0xf9002ba0
.inst 0xb9801b59
.inst 0xf9400340
.inst 0xf90027a0
.inst 0xf9400740
.inst 0xf9002ba0
.inst 0xb9802ba0
.inst 0xd280003e
.inst 0x6b1e001f
.inst 0x54000481
.inst 0xb98053a0
.inst 0x6b00033f
.inst 0x54000422
.inst 0xf94027a0
.inst 0x93407f21
.inst 0xb98053a2
.inst 0xeb01005f
.inst 0x10000011
.inst 0x54000529
.inst 0xd37df021
.inst 0x8b010001
.inst 0xf94013a0
.inst 0xd2800002
.inst 0xb9802ba3
.inst 0xeb1f007f
.inst 0x10000011
.inst 0x54000429
.inst 0xd37df042
.inst 0x8b020000
.inst 0xf9400000
.inst 0xf90033a0
.inst 0xd5033bbf
.inst 0xf94033a0
.inst 0xf9000020
.inst 0xd349fc21
.inst 0x92405821

adrp x16, mono_aot_System_Runtime_Numerics_got+0
add x16, x16, :lo12:mono_aot_System_Runtime_Numerics_got
ldr x2, [x16, 16]
.inst 0x8b020021
.inst 0xd280003e
.inst 0x3900003e
.inst 0x11000720
.inst 0xb9001b40
.inst 0x1400000b
.inst 0xf94013a0
.inst 0xf9001ba0
.inst 0xf94017a0
.inst 0xf9001fa0
.inst 0xf94023a0
.inst 0xf940100f
.inst 0xaa1a03e0
.inst 0xf9401ba1
.inst 0xf9401fa2
bl .Lp_5
.inst 0xa9416bb9
.inst 0x910003bf
.inst 0xa8c77bfd
.inst 0xd65f03c0
.inst 0xd2801960
.inst 0xaa1103e1
bl .Lp_2

	.size System_Collections_Generic_ValueListBuilder_1_T_REF_Append_System_ReadOnlySpan_1_T_REF,.-System_Collections_Generic_ValueListBuilder_1_T_REF_Append_System_ReadOnlySpan_1_T_REF
.Lme_7b:
.text 0
	.balign 16
.Lm_7c:
	.local System_Collections_Generic_ValueListBuilder_1_T_REF_AppendMultiChar_System_ReadOnlySpan_1_T_REF
	.type System_Collections_Generic_ValueListBuilder_1_T_REF_AppendMultiChar_System_ReadOnlySpan_1_T_REF,@function
System_Collections_Generic_ValueListBuilder_1_T_REF_AppendMultiChar_System_ReadOnlySpan_1_T_REF:
.inst 0xa9b77bfd
.inst 0x910003fd
.inst 0xa90163b7
.inst 0xa9026bb9
.inst 0xf9003faf
.inst 0xaa0003fa
.inst 0xa9030ba1

adrp x16, mono_aot_System_Runtime_Numerics_got+0
add x16, x16, :lo12:mono_aot_System_Runtime_Numerics_got
ldr x0, [x16, 56]
.inst 0xf9400011
.inst 0xb4000051
bl .Lp_1

adrp x16, mono_aot_System_Runtime_Numerics_got+0
add x16, x16, :lo12:mono_aot_System_Runtime_Numerics_got
ldr x1, [x16, 216]
.inst 0xf9403fa0
.inst 0xf9400c10
.inst 0xb5000050
bl .Lp_3
.inst 0xb9801b40
.inst 0xb9803ba1
.inst 0xb010000
.inst 0xeb1f035f
.inst 0x10000011
.inst 0x54000ae0
.inst 0xb9800b41
.inst 0x6b01001f
.inst 0x540001a9
.inst 0xeb1f035f
.inst 0x10000011
.inst 0x54000a20
.inst 0xb9800b40
.inst 0xb9801b41
.inst 0x4b010000
.inst 0xb9803ba1
.inst 0xb010001
.inst 0xf9403fa0
.inst 0xf940100f
.inst 0xaa1a03e0
bl .Lp_6
.inst 0x9100c3b9
.inst 0xeb1f035f
.inst 0x10000011
.inst 0x54000880
.inst 0xb9801b40
.inst 0xaa1a03f8
.inst 0xaa0003f7
.inst 0xb9800b41
.inst 0x6b01001f
.inst 0x540007a8
.inst 0xf9400300
.inst 0x2a1703e1
.inst 0xd37df021
.inst 0x8b010001
.inst 0xb9800b00
.inst 0x4b170000
.inst 0xd2800002
.inst 0xf90037a2
.inst 0xf9003ba2
.inst 0xf90037a1
.inst 0xb90073a0
.inst 0xf94037a0
.inst 0xf90023a0
.inst 0xf9403ba0
.inst 0xf90027a0
.inst 0xaa1903f8
.inst 0xf94023a0
.inst 0xf9002fa0
.inst 0xf94027a0
.inst 0xf90033a0
.inst 0xb9800b20
.inst 0xb98063a1
.inst 0x6b01001f
.inst 0x54000368
.inst 0xf9402fa2
.inst 0xf9400301
.inst 0xb9800b00
.inst 0x2a0003e0
.inst 0xaa0203f9
.inst 0xaa0103f8
.inst 0xaa0003f7
.inst 0xf9002bbf
.inst 0xeb1f001f
.inst 0x54000249
.inst 0xf9403fa0
.inst 0xf9401401
.inst 0x910143a0
.inst 0xf90043a0
.inst 0xaa0103e0
.inst 0xf9400021
.inst 0xf940cc30
.inst 0xd63f0200
.inst 0xf94043be
.inst 0xf90003c0
.inst 0xf9402ba3
.inst 0xaa1903e0
.inst 0xaa1803e1
.inst 0xaa1703e2
bl .Lp_7
.inst 0x14000002
bl .Lp_8
.inst 0xb9801b40
.inst 0xb9803ba1
.inst 0xb010000
.inst 0xb9001b40
.inst 0xa94163b7
.inst 0xa9426bb9
.inst 0x910003bf
.inst 0xa8c97bfd
.inst 0xd65f03c0
bl .Lp_9
.inst 0xd2801e40
.inst 0xaa1103e1
bl .Lp_2

	.size System_Collections_Generic_ValueListBuilder_1_T_REF_AppendMultiChar_System_ReadOnlySpan_1_T_REF,.-System_Collections_Generic_ValueListBuilder_1_T_REF_AppendMultiChar_System_ReadOnlySpan_1_T_REF
.Lme_7c:
.text 0
	.balign 16
.Lm_7d:
	.local System_Collections_Generic_ValueListBuilder_1_T_REF_Insert_int_System_ReadOnlySpan_1_T_REF
	.type System_Collections_Generic_ValueListBuilder_1_T_REF_Insert_int_System_ReadOnlySpan_1_T_REF,@function
System_Collections_Generic_ValueListBuilder_1_T_REF_Insert_int_System_ReadOnlySpan_1_T_REF:
.inst 0xa9b27bfd
.inst 0x910003fd
.inst 0xa90163b7
.inst 0xa9026bb9
.inst 0xf9005faf
.inst 0xaa0003fa
.inst 0xf9001ba1
.inst 0xa9038fa2

adrp x16, mono_aot_System_Runtime_Numerics_got+0
add x16, x16, :lo12:mono_aot_System_Runtime_Numerics_got
ldr x0, [x16, 56]
.inst 0xf9400011
.inst 0xb4000051
bl .Lp_1

adrp x16, mono_aot_System_Runtime_Numerics_got+0
add x16, x16, :lo12:mono_aot_System_Runtime_Numerics_got
ldr x1, [x16, 224]
.inst 0xf9405fa0
.inst 0xf9400c10
.inst 0xb5000050
bl .Lp_3
.inst 0xd2800000
.inst 0xf90063a0
.inst 0xf90067a0
.inst 0xb9801b40
.inst 0xb98043a1
.inst 0xb010000
.inst 0xeb1f035f
.inst 0x10000011
.inst 0x54001280
.inst 0xb9800b41
.inst 0x6b01001f
.inst 0x540000c9
.inst 0xb98043a1
.inst 0xf9405fa0
.inst 0xf940100f
.inst 0xaa1a03e0
bl .Lp_6
.inst 0xeb1f035f
.inst 0x10000011
.inst 0x54001120
.inst 0xb9801b41
.inst 0xaa1a03f9
.inst 0xd2a00018
.inst 0xaa0103f7
.inst 0x2a1803e0
.inst 0x2a0103e1
.inst 0x8b010000
.inst 0xb9800b41
.inst 0x2a0103e1
.inst 0xeb01001f
.inst 0x54000f88
.inst 0xf9400320
.inst 0x2a1803e1
.inst 0xd37df021
.inst 0x8b010000
.inst 0xd2800001
.inst 0xf90057a1
.inst 0xf9005ba1
.inst 0xf90057a0
.inst 0xb900b3b7
.inst 0xf94057a0
.inst 0xf90063a0
.inst 0xf9405ba0
.inst 0xf90067a0
.inst 0x910303b9
.inst 0xeb1f035f
.inst 0x10000011
.inst 0x54000da0
.inst 0xb98043a0
.inst 0xaa1a03f8
.inst 0xaa0003f7
.inst 0xb9800b41
.inst 0x6b01001f
.inst 0x54000cc8
.inst 0xf9400300
.inst 0x2a1703e1
.inst 0xd37df021
.inst 0x8b010001
.inst 0xb9800b00
.inst 0x4b170000
.inst 0xd2800002
.inst 0xf9004fa2
.inst 0xf90053a2
.inst 0xf9004fa1
.inst 0xb900a3a0
.inst 0xf9404fa0
.inst 0xf9002fa0
.inst 0xf94053a0
.inst 0xf90033a0
.inst 0xaa1903f8
.inst 0xf9402fa0
.inst 0xf90047a0
.inst 0xf94033a0
.inst 0xf9004ba0
.inst 0xb9800b20
.inst 0xb98093a1
.inst 0x6b01001f
.inst 0x54000368
.inst 0xf94047a2
.inst 0xf9400301
.inst 0xb9800b00
.inst 0x2a0003e0
.inst 0xaa0203f9
.inst 0xaa0103f8
.inst 0xaa0003f7
.inst 0xf90043bf
.inst 0xeb1f001f
.inst 0x54000249
.inst 0xf9405fa0
.inst 0xf9401401
.inst 0x910203a0
.inst 0xf9006ba0
.inst 0xaa0103e0
.inst 0xf9400021
.inst 0xf940cc30
.inst 0xd63f0200
.inst 0xf9406bbe
.inst 0xf90003c0
.inst 0xf94043a3
.inst 0xaa1903e0
.inst 0xaa1803e1
.inst 0xaa1703e2
bl .Lp_7
.inst 0x14000002
bl .Lp_8
.inst 0xf9400340
.inst 0xf90027a0
.inst 0xf9400740
.inst 0xf9002ba0
.inst 0x9100e3b9
.inst 0xf94027a0
.inst 0xf9003ba0
.inst 0xf9402ba0
.inst 0xf9003fa0
.inst 0xb98043a0
.inst 0xb9807ba1
.inst 0x6b01001f
.inst 0x54000368
.inst 0xf9403ba2
.inst 0xf9400321
.inst 0xb9800b20
.inst 0x2a0003e0
.inst 0xaa0203f9
.inst 0xaa0103f8
.inst 0xaa0003f7
.inst 0xf90037bf
.inst 0xeb1f001f
.inst 0x54000249
.inst 0xf9405fa0
.inst 0xf9401401
.inst 0x9101a3a0
.inst 0xf9006ba0
.inst 0xaa0103e0
.inst 0xf9400021
.inst 0xf940cc30
.inst 0xd63f0200
.inst 0xf9406bbe
.inst 0xf90003c0
.inst 0xf94037a3
.inst 0xaa1903e0
.inst 0xaa1803e1
.inst 0xaa1703e2
bl .Lp_7
.inst 0x14000002
bl .Lp_8
.inst 0xb9801b40
.inst 0xb98043a1
.inst 0xb010000
.inst 0xb9001b40
.inst 0xa94163b7
.inst 0xa9426bb9
.inst 0x910003bf
.inst 0xa8ce7bfd
.inst 0xd65f03c0
bl .Lp_9
bl .Lp_9
.inst 0xd2801e40
.inst 0xaa1103e1
bl .Lp_2

	.size System_Collections_Generic_ValueListBuilder_1_T_REF_Insert_int_System_ReadOnlySpan_1_T_REF,.-System_Collections_Generic_ValueListBuilder_1_T_REF_Insert_int_System_ReadOnlySpan_1_T_REF
.Lme_7d:
.text 0
	.balign 16
.Lm_7e:
	.local System_Collections_Generic_ValueListBuilder_1_T_REF_AppendSpan_int
	.type System_Collections_Generic_ValueListBuilder_1_T_REF_AppendSpan_int,@function
System_Collections_Generic_ValueListBuilder_1_T_REF_AppendSpan_int:
.inst 0xa9b97bfd
.inst 0x910003fd
.inst 0xa90163b7
.inst 0xa9026bb9
.inst 0xf9002baf
.inst 0xaa0003f9
.inst 0xaa0103fa

adrp x16, mono_aot_System_Runtime_Numerics_got+0
add x16, x16, :lo12:mono_aot_System_Runtime_Numerics_got
ldr x0, [x16, 56]
.inst 0xf9400011
.inst 0xb4000051
bl .Lp_1

adrp x16, mono_aot_System_Runtime_Numerics_got+0
add x16, x16, :lo12:mono_aot_System_Runtime_Numerics_got
ldr x1, [x16, 232]
.inst 0xf9402ba0
.inst 0xf9400c10
.inst 0xb5000050
bl .Lp_3
.inst 0xd2800000
.inst 0xf9002fa0
.inst 0xf90033a0
.inst 0xb9801b38
.inst 0xf9400320
.inst 0xf9002fa0
.inst 0xf9400720
.inst 0xf90033a0
.inst 0x2a1803e0
.inst 0x2a1a03e1
.inst 0x8b010000
.inst 0xb98063a1
.inst 0x2a0103e1
.inst 0xeb01001f
.inst 0x54000368
.inst 0xb1a0300
.inst 0xb9001b20
.inst 0x910163b9
.inst 0xaa1803f7
.inst 0xaa1a03f8
.inst 0x2a1703e0
.inst 0x2a1a03e1
.inst 0x8b010000
.inst 0xb98063a1
.inst 0x2a0103e1
.inst 0xeb01001f
.inst 0x540003c8
.inst 0xf9400320
.inst 0x2a1703e1
.inst 0xd37df021
.inst 0x8b010000
.inst 0xd2800001
.inst 0xf90023a1
.inst 0xf90027a1
.inst 0xf90023a0
.inst 0xb9004bb8
.inst 0xf94023a0
.inst 0xf9001ba0
.inst 0xf94027a0
.inst 0xf9001fa0
.inst 0x1400000a
.inst 0xf9402ba0
.inst 0xf940100f
.inst 0x9100c3a0
.inst 0xf90037a0
.inst 0xaa1903e0
.inst 0xaa1a03e1
bl .Lp_10
.inst 0xf94037be
.inst 0xa90007c0
.inst 0xa94163b7
.inst 0xa9426bb9
.inst 0xa94307a0
.inst 0x910003bf
.inst 0xa8c77bfd
.inst 0xd65f03c0
bl .Lp_9

	.size System_Collections_Generic_ValueListBuilder_1_T_REF_AppendSpan_int,.-System_Collections_Generic_ValueListBuilder_1_T_REF_AppendSpan_int
.Lme_7e:
.text 0
	.balign 16
.Lm_7f:
	.local System_Collections_Generic_ValueListBuilder_1_T_REF_AppendSpanWithGrow_int
	.type System_Collections_Generic_ValueListBuilder_1_T_REF_AppendSpanWithGrow_int,@function
System_Collections_Generic_ValueListBuilder_1_T_REF_AppendSpanWithGrow_int:
.inst 0xa9b97bfd
.inst 0x910003fd
.inst 0xa90163b7
.inst 0xf90013b9
.inst 0xf9002baf
.inst 0xaa0003f9
.inst 0xf9001fa1

adrp x16, mono_aot_System_Runtime_Numerics_got+0
add x16, x16, :lo12:mono_aot_System_Runtime_Numerics_got
ldr x0, [x16, 56]
.inst 0xf9400011
.inst 0xb4000051
bl .Lp_1

adrp x16, mono_aot_System_Runtime_Numerics_got+0
add x16, x16, :lo12:mono_aot_System_Runtime_Numerics_got
ldr x1, [x16, 240]
.inst 0xf9402ba0
.inst 0xf9400c10
.inst 0xb5000050
bl .Lp_3
.inst 0xb9801b21
.inst 0xeb1f033f
.inst 0x10000011
.inst 0x54000600
.inst 0xb9800b20
.inst 0xf90033a1
.inst 0x4b010000
.inst 0xb9803ba1
.inst 0xb010001
.inst 0xf9402ba0
.inst 0xf940100f
.inst 0xaa1903e0
bl .Lp_6
.inst 0xf94033a0
.inst 0xb9801b22
.inst 0xb9803ba1
.inst 0xb010042
.inst 0xb9001b22
.inst 0xeb1f033f
.inst 0x10000011
.inst 0x540003e0
.inst 0xaa1903f8
.inst 0xaa0003f9
.inst 0xaa0103f7
.inst 0x2a1903e0
.inst 0x2a0103e1
.inst 0x8b010000
.inst 0xb9800b01
.inst 0x2a0103e1
.inst 0xeb01001f
.inst 0x54000288
.inst 0xf9400300
.inst 0x2a1903e1
.inst 0xd37df021
.inst 0x8b010000
.inst 0xd2800001
.inst 0xf90023a1
.inst 0xf90027a1
.inst 0xf90023a0
.inst 0xb9004bb7
.inst 0xf94023a0
.inst 0xf90017a0
.inst 0xf94027a0
.inst 0xf9001ba0
.inst 0xa94163b7
.inst 0xf94013b9
.inst 0xa94287a0
.inst 0x910003bf
.inst 0xa8c77bfd
.inst 0xd65f03c0
bl .Lp_9
.inst 0xd2801e40
.inst 0xaa1103e1
bl .Lp_2

	.size System_Collections_Generic_ValueListBuilder_1_T_REF_AppendSpanWithGrow_int,.-System_Collections_Generic_ValueListBuilder_1_T_REF_AppendSpanWithGrow_int
.Lme_7f:
.text 0
	.balign 16
.Lm_80:
	.local System_Collections_Generic_ValueListBuilder_1_T_REF_AddWithResize_T_REF
	.type System_Collections_Generic_ValueListBuilder_1_T_REF_AddWithResize_T_REF,@function
System_Collections_Generic_ValueListBuilder_1_T_REF_AddWithResize_T_REF:
.inst 0xa9bc7bfd
.inst 0x910003fd
.inst 0xf9000bb9
.inst 0xf90013af
.inst 0xaa0003f9
.inst 0xf9000fa1

adrp x16, mono_aot_System_Runtime_Numerics_got+0
add x16, x16, :lo12:mono_aot_System_Runtime_Numerics_got
ldr x0, [x16, 56]
.inst 0xf9400011
.inst 0xb4000051
bl .Lp_1

adrp x16, mono_aot_System_Runtime_Numerics_got+0
add x16, x16, :lo12:mono_aot_System_Runtime_Numerics_got
ldr x1, [x16, 248]
.inst 0xf94013a0
.inst 0xf9400c10
.inst 0xb5000050
bl .Lp_3
.inst 0xb9801b20
.inst 0xf9001fa0
.inst 0xf94013a0
.inst 0xf940100f
.inst 0xaa1903e0
.inst 0xd2800021
bl .Lp_6
.inst 0xf9401fa1
.inst 0xeb1f033f
.inst 0x10000011
.inst 0x540003e0
.inst 0xf9400320
.inst 0xf9001ba1
.inst 0x93407c21
.inst 0xb9800b22
.inst 0xeb01005f
.inst 0x10000011
.inst 0x540002a9
.inst 0xd37df021
.inst 0x8b010002
.inst 0xd5033bbf
.inst 0xf9401ba0
.inst 0xf9400fa1
.inst 0xf9000041
.inst 0xd349fc42
.inst 0x92405842

adrp x16, mono_aot_System_Runtime_Numerics_got+0
add x16, x16, :lo12:mono_aot_System_Runtime_Numerics_got
ldr x3, [x16, 16]
.inst 0x8b030042
.inst 0xd280003e
.inst 0x3900005e
.inst 0x11000400
.inst 0xb9001b20
.inst 0xf9400bb9
.inst 0x910003bf
.inst 0xa8c47bfd
.inst 0xd65f03c0
.inst 0xd2801960
.inst 0xaa1103e1
bl .Lp_2
.inst 0xd2801e40
.inst 0xaa1103e1
bl .Lp_2

	.size System_Collections_Generic_ValueListBuilder_1_T_REF_AddWithResize_T_REF,.-System_Collections_Generic_ValueListBuilder_1_T_REF_AddWithResize_T_REF
.Lme_80:
.text 0
	.balign 16
.Lm_81:
	.local System_Collections_Generic_ValueListBuilder_1_T_REF_AsSpan
	.type System_Collections_Generic_ValueListBuilder_1_T_REF_AsSpan,@function
System_Collections_Generic_ValueListBuilder_1_T_REF_AsSpan:
.inst 0xa9b97bfd
.inst 0x910003fd
.inst 0xa90167b8
.inst 0xf90013ba
.inst 0xf90037af
.inst 0xaa0003fa

adrp x16, mono_aot_System_Runtime_Numerics_got+0
add x16, x16, :lo12:mono_aot_System_Runtime_Numerics_got
ldr x0, [x16, 56]
.inst 0xf9400011
.inst 0xb4000051
bl .Lp_1
.inst 0xeb1f035f
.inst 0x10000011
.inst 0x54000560
.inst 0xb9801b41
.inst 0xaa1a03f9
.inst 0xd2a0001a
.inst 0xaa0103f8
.inst 0x2a1a03e0
.inst 0x2a0103e1
.inst 0x8b010000
.inst 0xb9800b21
.inst 0x2a0103e1
.inst 0xeb01001f
.inst 0x540003e8
.inst 0xf9400320
.inst 0x2a1a03e1
.inst 0xd37df021
.inst 0x8b010000
.inst 0xd2800001
.inst 0xf9002fa1
.inst 0xf90033a1
.inst 0xf9002fa0
.inst 0xb90063b8
.inst 0xf9402fa0
.inst 0xf90027a0
.inst 0xf94033a0
.inst 0xf9002ba0
.inst 0xf94027a1
.inst 0xb98053a0
.inst 0xd2800002
.inst 0xf9001fa2
.inst 0xf90023a2
.inst 0xf9001fa1
.inst 0xb90043a0
.inst 0xf9401fa0
.inst 0xf90017a0
.inst 0xf94023a0
.inst 0xf9001ba0
.inst 0xa94167b8
.inst 0xf94013ba
.inst 0xa94287a0
.inst 0x910003bf
.inst 0xa8c77bfd
.inst 0xd65f03c0
bl .Lp_9
.inst 0xd2801e40
.inst 0xaa1103e1
bl .Lp_2

	.size System_Collections_Generic_ValueListBuilder_1_T_REF_AsSpan,.-System_Collections_Generic_ValueListBuilder_1_T_REF_AsSpan
.Lme_81:
.text 0
	.balign 16
.Lm_82:
	.local System_Collections_Generic_ValueListBuilder_1_T_REF_TryCopyTo_System_Span_1_T_REF_int_
	.type System_Collections_Generic_ValueListBuilder_1_T_REF_TryCopyTo_System_Span_1_T_REF_int_,@function
System_Collections_Generic_ValueListBuilder_1_T_REF_TryCopyTo_System_Span_1_T_REF_int_:
.inst 0xa9b97bfd
.inst 0x910003fd
.inst 0xa9015fb6
.inst 0xa90267b8
.inst 0xf9002faf
.inst 0xaa0003f9
.inst 0xa9030ba1
.inst 0xf90023a3

adrp x16, mono_aot_System_Runtime_Numerics_got+0
add x16, x16, :lo12:mono_aot_System_Runtime_Numerics_got
ldr x0, [x16, 56]
.inst 0xf9400011
.inst 0xb4000051
bl .Lp_1

adrp x16, mono_aot_System_Runtime_Numerics_got+0
add x16, x16, :lo12:mono_aot_System_Runtime_Numerics_got
ldr x1, [x16, 256]
.inst 0xf9402fa0
.inst 0xf9400c10
.inst 0xb5000050
bl .Lp_3
.inst 0xd2800000
.inst 0xf90033a0
.inst 0xf90037a0
.inst 0xeb1f033f
.inst 0x10000011
.inst 0x540005e0
.inst 0xb9801b21
.inst 0xaa1903f8
.inst 0xd2a00017
.inst 0xaa0103f6
.inst 0x2a1703e0
.inst 0x2a0103e1
.inst 0x8b010000
.inst 0xb9800b21
.inst 0x2a0103e1
.inst 0xeb01001f
.inst 0x54000468
.inst 0xf9400300
.inst 0x2a1703e1
.inst 0xd37df021
.inst 0x8b010000
.inst 0xd2800001
.inst 0xf90027a1
.inst 0xf9002ba1
.inst 0xf90027a0
.inst 0xb90053b6
.inst 0xf94027a0
.inst 0xf90033a0
.inst 0xf9402ba0
.inst 0xf90037a0
.inst 0xf9402fa0
.inst 0xf940100f
.inst 0x910183a0
.inst 0xf9401ba1
.inst 0xf9401fa2
bl .Lp_11
.inst 0x53001c00
.inst 0x340000c0
.inst 0xb9801b21
.inst 0xf94023a0
.inst 0xb9000001
.inst 0xd2800020
.inst 0x14000004
.inst 0xf94023a0
.inst 0xb900001f
.inst 0xd2a00000
.inst 0xa9415fb6
.inst 0xa94267b8
.inst 0x910003bf
.inst 0xa8c77bfd
.inst 0xd65f03c0
bl .Lp_9
.inst 0xd2801e40
.inst 0xaa1103e1
bl .Lp_2

	.size System_Collections_Generic_ValueListBuilder_1_T_REF_TryCopyTo_System_Span_1_T_REF_int_,.-System_Collections_Generic_ValueListBuilder_1_T_REF_TryCopyTo_System_Span_1_T_REF_int_
.Lme_82:
.text 0
	.balign 16
.Lm_83:
	.local System_Collections_Generic_ValueListBuilder_1_T_REF_Dispose
	.type System_Collections_Generic_ValueListBuilder_1_T_REF_Dispose,@function
System_Collections_Generic_ValueListBuilder_1_T_REF_Dispose:
.inst 0xa9bd7bfd
.inst 0x910003fd
.inst 0xf9000bb9
.inst 0xf90013af
.inst 0xf9000fa0

adrp x16, mono_aot_System_Runtime_Numerics_got+0
add x16, x16, :lo12:mono_aot_System_Runtime_Numerics_got
ldr x0, [x16, 56]
.inst 0xf9400011
.inst 0xb4000051
bl .Lp_1

adrp x16, mono_aot_System_Runtime_Numerics_got+0
add x16, x16, :lo12:mono_aot_System_Runtime_Numerics_got
ldr x1, [x16, 264]
.inst 0xf94013a0
.inst 0xf9400c10
.inst 0xb5000050
bl .Lp_3
.inst 0xf9400fa0
.inst 0xf9400819
.inst 0xaa1903e0
.inst 0xb40002e0
.inst 0xf9400fa0
.inst 0xf900081f
.inst 0xf94013a0
.inst 0xf9401400
bl .Lp_12
.inst 0x53001c00
.inst 0x350000c0
.inst 0xf9400fa0
.inst 0xb9801802
.inst 0xaa1903e0
.inst 0xd2a00001
bl .Lp_13
.inst 0xf94013a0
.inst 0xf940100f
bl .Lp_14
.inst 0xaa0003e3
.inst 0xaa0303e0
.inst 0xaa1903e1
.inst 0xd2a00002
.inst 0xf9400063
.inst 0xf9403870
.inst 0xd63f0200
.inst 0xf9400bb9
.inst 0x910003bf
.inst 0xa8c37bfd
.inst 0xd65f03c0

	.size System_Collections_Generic_ValueListBuilder_1_T_REF_Dispose,.-System_Collections_Generic_ValueListBuilder_1_T_REF_Dispose
.Lme_83:
.text 0
	.balign 16
.Lm_84:
	.local System_Collections_Generic_ValueListBuilder_1_T_REF_Grow_int
	.type System_Collections_Generic_ValueListBuilder_1_T_REF_Grow_int,@function
System_Collections_Generic_ValueListBuilder_1_T_REF_Grow_int:
.inst 0xa9b57bfd
.inst 0x910003fd
.inst 0xa9015fb6
.inst 0xa90267b8
.inst 0xf9001bba
.inst 0xf9004baf
.inst 0xaa0003f9
.inst 0xaa0103fa

adrp x16, mono_aot_System_Runtime_Numerics_got+0
add x16, x16, :lo12:mono_aot_System_Runtime_Numerics_got
ldr x0, [x16, 56]
.inst 0xf9400011
.inst 0xb4000051
bl .Lp_1

adrp x16, mono_aot_System_Runtime_Numerics_got+0
add x16, x16, :lo12:mono_aot_System_Runtime_Numerics_got
ldr x1, [x16, 272]
.inst 0xf9404ba0
.inst 0xf9400c10
.inst 0xb5000050
bl .Lp_3
.inst 0xeb1f033f
.inst 0x10000011
.inst 0x54001980
.inst 0xb9800b20
.inst 0x35000060
.inst 0xd2800098
.inst 0x14000006
.inst 0xeb1f033f
.inst 0x10000011
.inst 0x540018a0
.inst 0xb9800b20
.inst 0x531f7818
.inst 0xeb1f033f
.inst 0x10000011
.inst 0x54001800
.inst 0xb9800b20
.inst 0xb1a0000
.inst 0xaa1803fa
.inst 0xaa0003f8
.inst 0x6b00035f
.inst 0x5400004a
.inst 0x14000002
.inst 0xaa1a03f8
.inst 0xaa1803fa
.inst 0xd29ff8fe
.inst 0xf2affffe
.inst 0x6b1e031f
.inst 0x54000329
.inst 0xeb1f033f
.inst 0x10000011
.inst 0x54001600
.inst 0xb9800b20
.inst 0x1100041a
.inst 0xd29ff8f8
.inst 0xf2affff8
.inst 0xd29ff8fe
.inst 0xf2affffe
.inst 0x6b1e035f
.inst 0x5400004a
.inst 0x14000002
.inst 0xaa1a03f8
.inst 0xeb1f033f
.inst 0x10000011
.inst 0x54001460
.inst 0xb9800b20
.inst 0xaa1803fa
.inst 0xaa0003f8
.inst 0x6b00035f
.inst 0x5400004a
.inst 0x14000002
.inst 0xaa1a03f8
.inst 0xaa1803fa
.inst 0xf9404ba0
.inst 0xf940100f
bl .Lp_14
.inst 0xaa0003e2
.inst 0xaa0203e0
.inst 0xaa1a03e1
.inst 0xf9400042
.inst 0xf9403c50
.inst 0xd63f0200
.inst 0xaa0003fa
.inst 0xeb1f033f
.inst 0x10000011
.inst 0x540011c0
.inst 0xaa1903f8
.inst 0xd2800000
.inst 0xf90043a0
.inst 0xf90047a0
.inst 0x910203b7
.inst 0xaa1a03f6
.inst 0xb50000ba
.inst 0xd2800000
.inst 0xf90002e0
.inst 0xf90006e0
.inst 0x1400000d
.inst 0xf94002c0
.inst 0xf9400c00
.inst 0xf9404ba1
.inst 0xf9401421
.inst 0xeb01001f
.inst 0x9a9f07e0
.inst 0x35000f40
.inst 0x394002de
.inst 0x910082c0
.inst 0xf90002e0
.inst 0xb9801ac0
.inst 0xb9000ae0
.inst 0xf94043a0
.inst 0xf90027a0
.inst 0xf94047a0
.inst 0xf9002ba0
.inst 0xaa1803f7
.inst 0xf94027a0
.inst 0xf9003ba0
.inst 0xf9402ba0
.inst 0xf9003fa0
.inst 0xb9800b00
.inst 0xb9807ba1
.inst 0x6b01001f
.inst 0x54000368
.inst 0xf9403ba2
.inst 0xf94002e1
.inst 0xb9800ae0
.inst 0x2a0003e0
.inst 0xaa0203f8
.inst 0xaa0103f7
.inst 0xaa0003f6
.inst 0xf90037bf
.inst 0xeb1f001f
.inst 0x54000249
.inst 0xf9404ba0
.inst 0xf9401801
.inst 0x9101a3a0
.inst 0xf9004fa0
.inst 0xaa0103e0
.inst 0xf9400021
.inst 0xf940cc30
.inst 0xd63f0200
.inst 0xf9404fbe
.inst 0xf90003c0
.inst 0xf94037a3
.inst 0xaa1803e0
.inst 0xaa1703e1
.inst 0xaa1603e2
bl .Lp_7
.inst 0x14000002
bl .Lp_8
.inst 0xf9400b38
.inst 0xaa1903f7
.inst 0xaa1a03f6
.inst 0x91004320
.inst 0xf90053a0
.inst 0xd5033bbf
.inst 0xf94053a0
.inst 0xf900001a
.inst 0xd349fc00
.inst 0x92405800

adrp x16, mono_aot_System_Runtime_Numerics_got+0
add x16, x16, :lo12:mono_aot_System_Runtime_Numerics_got
ldr x1, [x16, 16]
.inst 0x8b010000
.inst 0xd280003e
.inst 0x3900001e
.inst 0xaa1a03e0
.inst 0xd2800001
.inst 0xf9002fa1
.inst 0xf90033a1
.inst 0x910163ba
.inst 0xaa0003f6
.inst 0xb50000a0
.inst 0xd2800000
.inst 0xf9000340
.inst 0xf9000740
.inst 0x1400000d
.inst 0xf94002c0
.inst 0xf9400c00
.inst 0xf9404ba1
.inst 0xf9401421
.inst 0xeb01001f
.inst 0x9a9f07e0
.inst 0x35000580
.inst 0x394002de
.inst 0x910082c0
.inst 0xf9000340
.inst 0xb9801ac0
.inst 0xb9000b40
.inst 0xf9402fa0
.inst 0xf9001fa0
.inst 0xf94033a0
.inst 0xf90023a0
.inst 0xeb1f02ff
.inst 0x10000011
.inst 0x54000420
.inst 0xf9401fa0
.inst 0xf90002e0
.inst 0xf94023a0
.inst 0xf90006e0
.inst 0xb4000298
.inst 0xf9404ba0
.inst 0xf9401800
bl .Lp_12
.inst 0x53001c00
.inst 0x350000a0
.inst 0xb9801b22
.inst 0xaa1803e0
.inst 0xd2a00001
bl .Lp_13
.inst 0xf9404ba0
.inst 0xf940100f
bl .Lp_14
.inst 0xaa0003e3
.inst 0xaa0303e0
.inst 0xaa1803e1
.inst 0xd2a00002
.inst 0xf9400063
.inst 0xf9403870
.inst 0xd63f0200
.inst 0xa9415fb6
.inst 0xa94267b8
.inst 0xf9401bba
.inst 0x910003bf
.inst 0xa8cb7bfd
.inst 0xd65f03c0
bl .Lp_15
bl .Lp_15
.inst 0xd2801e40
.inst 0xaa1103e1
bl .Lp_2

	.size System_Collections_Generic_ValueListBuilder_1_T_REF_Grow_int,.-System_Collections_Generic_ValueListBuilder_1_T_REF_Grow_int
.Lme_84:
.text 0
	.balign 16
.Lm_129:
	.local System_Numerics_BigInteger_CreateChecked_TOther_REF_TOther_REF
	.type System_Numerics_BigInteger_CreateChecked_TOther_REF_TOther_REF,@function
System_Numerics_BigInteger_CreateChecked_TOther_REF_TOther_REF:
.inst 0xa9ad7bfd
.inst 0x910003fd
.inst 0xf90017af
.inst 0xf90013a0

adrp x16, mono_aot_System_Runtime_Numerics_got+0
add x16, x16, :lo12:mono_aot_System_Runtime_Numerics_got
ldr x0, [x16, 56]
.inst 0xf9400011
.inst 0xb4000051
bl .Lp_1

adrp x16, mono_aot_System_Runtime_Numerics_got+0
add x16, x16, :lo12:mono_aot_System_Runtime_Numerics_got
ldr x1, [x16, 280]
.inst 0xf94017a0
.inst 0xf9400c10
.inst 0xb5000050
bl .Lp_3
.inst 0xd2800000
.inst 0xf9001ba0
.inst 0xf9001fa0
.inst 0xd2800000
.inst 0xf9001ba0
.inst 0xf9001fa0
.inst 0xd2a00000
.inst 0x53001c00
.inst 0x35000180
.inst 0xf94017a0
.inst 0xf9401002
.inst 0xf94013a0
.inst 0x9100c3a1
.inst 0xd63f0040
.inst 0x53001c00
.inst 0x350000a0
.inst 0xd2801e00
.inst 0xf2a04000
bl .Lp_16
bl .Lp_17
.inst 0xf9401ba0
.inst 0xf9000ba0
.inst 0xf9401fa0
.inst 0xf9000fa0
.inst 0xa94107a0
.inst 0x910003bf
.inst 0xa8d37bfd
.inst 0xd65f03c0

	.size System_Numerics_BigInteger_CreateChecked_TOther_REF_TOther_REF,.-System_Numerics_BigInteger_CreateChecked_TOther_REF_TOther_REF
.Lme_129:
.text 0
	.balign 16
.Lm_12a:
	.local System_Numerics_BigInteger_CreateSaturating_TOther_REF_TOther_REF
	.type System_Numerics_BigInteger_CreateSaturating_TOther_REF_TOther_REF,@function
System_Numerics_BigInteger_CreateSaturating_TOther_REF_TOther_REF:
.inst 0xa9ad7bfd
.inst 0x910003fd
.inst 0xf90017af
.inst 0xf90013a0

adrp x16, mono_aot_System_Runtime_Numerics_got+0
add x16, x16, :lo12:mono_aot_System_Runtime_Numerics_got
ldr x0, [x16, 56]
.inst 0xf9400011
.inst 0xb4000051
bl .Lp_1

adrp x16, mono_aot_System_Runtime_Numerics_got+0
add x16, x16, :lo12:mono_aot_System_Runtime_Numerics_got
ldr x1, [x16, 288]
.inst 0xf94017a0
.inst 0xf9400c10
.inst 0xb5000050
bl .Lp_3
.inst 0xd2800000
.inst 0xf9001ba0
.inst 0xf9001fa0
.inst 0xd2800000
.inst 0xf9001ba0
.inst 0xf9001fa0
.inst 0xd2a00000
.inst 0x53001c00
.inst 0x35000180
.inst 0xf94017a0
.inst 0xf9401002
.inst 0xf94013a0
.inst 0x9100c3a1
.inst 0xd63f0040
.inst 0x53001c00
.inst 0x350000a0
.inst 0xd2801e00
.inst 0xf2a04000
bl .Lp_16
bl .Lp_17
.inst 0xf9401ba0
.inst 0xf9000ba0
.inst 0xf9401fa0
.inst 0xf9000fa0
.inst 0xa94107a0
.inst 0x910003bf
.inst 0xa8d37bfd
.inst 0xd65f03c0

	.size System_Numerics_BigInteger_CreateSaturating_TOther_REF_TOther_REF,.-System_Numerics_BigInteger_CreateSaturating_TOther_REF_TOther_REF
.Lme_12a:
.text 0
	.balign 16
.Lm_12b:
	.local System_Numerics_BigInteger_CreateTruncating_TOther_REF_TOther_REF
	.type System_Numerics_BigInteger_CreateTruncating_TOther_REF_TOther_REF,@function
System_Numerics_BigInteger_CreateTruncating_TOther_REF_TOther_REF:
.inst 0xa9ad7bfd
.inst 0x910003fd
.inst 0xf90017af
.inst 0xf90013a0

adrp x16, mono_aot_System_Runtime_Numerics_got+0
add x16, x16, :lo12:mono_aot_System_Runtime_Numerics_got
ldr x0, [x16, 56]
.inst 0xf9400011
.inst 0xb4000051
bl .Lp_1

adrp x16, mono_aot_System_Runtime_Numerics_got+0
add x16, x16, :lo12:mono_aot_System_Runtime_Numerics_got
ldr x1, [x16, 296]
.inst 0xf94017a0
.inst 0xf9400c10
.inst 0xb5000050
bl .Lp_3
.inst 0xd2800000
.inst 0xf9001ba0
.inst 0xf9001fa0
.inst 0xd2800000
.inst 0xf9001ba0
.inst 0xf9001fa0
.inst 0xd2a00000
.inst 0x53001c00
.inst 0x35000180
.inst 0xf94017a0
.inst 0xf9401002
.inst 0xf94013a0
.inst 0x9100c3a1
.inst 0xd63f0040
.inst 0x53001c00
.inst 0x350000a0
.inst 0xd2801e00
.inst 0xf2a04000
bl .Lp_16
bl .Lp_17
.inst 0xf9401ba0
.inst 0xf9000ba0
.inst 0xf9401fa0
.inst 0xf9000fa0
.inst 0xa94107a0
.inst 0x910003bf
.inst 0xa8d37bfd
.inst 0xd65f03c0

	.size System_Numerics_BigInteger_CreateTruncating_TOther_REF_TOther_REF,.-System_Numerics_BigInteger_CreateTruncating_TOther_REF_TOther_REF
.Lme_12b:
.text 0
	.balign 16
.Lm_130:
	.local System_Numerics_BigInteger_System_Numerics_INumberBase_System_Numerics_BigInteger_TryConvertFromChecked_TOther_REF_TOther_REF_System_Numerics_BigInteger_
	.type System_Numerics_BigInteger_System_Numerics_INumberBase_System_Numerics_BigInteger_TryConvertFromChecked_TOther_REF_TOther_REF_System_Numerics_BigInteger_,@function
System_Numerics_BigInteger_System_Numerics_INumberBase_System_Numerics_BigInteger_TryConvertFromChecked_TOther_REF_TOther_REF_System_Numerics_BigInteger_:
.inst 0xa9af7bfd
.inst 0x910003fd
.inst 0xf90013af
.inst 0xf9000ba0
.inst 0xf9000fa1

adrp x16, mono_aot_System_Runtime_Numerics_got+0
add x16, x16, :lo12:mono_aot_System_Runtime_Numerics_got
ldr x0, [x16, 56]
.inst 0xf9400011
.inst 0xb4000051
bl .Lp_1
.inst 0xf9400fa0
.inst 0xd2800001
.inst 0xf9000001
.inst 0xf9000401
.inst 0xd2a00000
.inst 0x53001c00
.inst 0x910003bf
.inst 0xa8d17bfd
.inst 0xd65f03c0

	.size System_Numerics_BigInteger_System_Numerics_INumberBase_System_Numerics_BigInteger_TryConvertFromChecked_TOther_REF_TOther_REF_System_Numerics_BigInteger_,.-System_Numerics_BigInteger_System_Numerics_INumberBase_System_Numerics_BigInteger_TryConvertFromChecked_TOther_REF_TOther_REF_System_Numerics_BigInteger_
.Lme_130:
.text 0
	.balign 16
.Lm_131:
	.local System_Numerics_BigInteger_TryConvertFromChecked_TOther_REF_TOther_REF_System_Numerics_BigInteger_
	.type System_Numerics_BigInteger_TryConvertFromChecked_TOther_REF_TOther_REF_System_Numerics_BigInteger_,@function
System_Numerics_BigInteger_TryConvertFromChecked_TOther_REF_TOther_REF_System_Numerics_BigInteger_:
.inst 0xa9af7bfd
.inst 0x910003fd
.inst 0xf90013af
.inst 0xf9000ba0
.inst 0xf9000fa1

adrp x16, mono_aot_System_Runtime_Numerics_got+0
add x16, x16, :lo12:mono_aot_System_Runtime_Numerics_got
ldr x0, [x16, 56]
.inst 0xf9400011
.inst 0xb4000051
bl .Lp_1
.inst 0xd2800001
.inst 0xf9400fa0
.inst 0xf9000001
.inst 0xf9000401
.inst 0xd2a00000
.inst 0x910003bf
.inst 0xa8d17bfd
.inst 0xd65f03c0

	.size System_Numerics_BigInteger_TryConvertFromChecked_TOther_REF_TOther_REF_System_Numerics_BigInteger_,.-System_Numerics_BigInteger_TryConvertFromChecked_TOther_REF_TOther_REF_System_Numerics_BigInteger_
.Lme_131:
.text 0
	.balign 16
.Lm_132:
	.local System_Numerics_BigInteger_System_Numerics_INumberBase_System_Numerics_BigInteger_TryConvertFromSaturating_TOther_REF_TOther_REF_System_Numerics_BigInteger_
	.type System_Numerics_BigInteger_System_Numerics_INumberBase_System_Numerics_BigInteger_TryConvertFromSaturating_TOther_REF_TOther_REF_System_Numerics_BigInteger_,@function
System_Numerics_BigInteger_System_Numerics_INumberBase_System_Numerics_BigInteger_TryConvertFromSaturating_TOther_REF_TOther_REF_System_Numerics_BigInteger_:
.inst 0xa9ae7bfd
.inst 0x910003fd
.inst 0xf90013af
.inst 0xf9000ba0
.inst 0xf9000fa1

adrp x16, mono_aot_System_Runtime_Numerics_got+0
add x16, x16, :lo12:mono_aot_System_Runtime_Numerics_got
ldr x0, [x16, 56]
.inst 0xf9400011
.inst 0xb4000051
bl .Lp_1
.inst 0xf9400fa0
.inst 0xd2800001
.inst 0xf9000001
.inst 0xf9000401
.inst 0xd2a00000
.inst 0x53001c00
.inst 0x910003bf
.inst 0xa8d27bfd
.inst 0xd65f03c0

	.size System_Numerics_BigInteger_System_Numerics_INumberBase_System_Numerics_BigInteger_TryConvertFromSaturating_TOther_REF_TOther_REF_System_Numerics_BigInteger_,.-System_Numerics_BigInteger_System_Numerics_INumberBase_System_Numerics_BigInteger_TryConvertFromSaturating_TOther_REF_TOther_REF_System_Numerics_BigInteger_
.Lme_132:
.text 0
	.balign 16
.Lm_133:
	.local System_Numerics_BigInteger_TryConvertFromSaturating_TOther_REF_TOther_REF_System_Numerics_BigInteger_
	.type System_Numerics_BigInteger_TryConvertFromSaturating_TOther_REF_TOther_REF_System_Numerics_BigInteger_,@function
System_Numerics_BigInteger_TryConvertFromSaturating_TOther_REF_TOther_REF_System_Numerics_BigInteger_:
.inst 0xa9ae7bfd
.inst 0x910003fd
.inst 0xf90013af
.inst 0xf9000ba0
.inst 0xf9000fa1

adrp x16, mono_aot_System_Runtime_Numerics_got+0
add x16, x16, :lo12:mono_aot_System_Runtime_Numerics_got
ldr x0, [x16, 56]
.inst 0xf9400011
.inst 0xb4000051
bl .Lp_1
.inst 0xd2800001
.inst 0xf9400fa0
.inst 0xf9000001
.inst 0xf9000401
.inst 0xd2a00000
.inst 0x910003bf
.inst 0xa8d27bfd
.inst 0xd65f03c0

	.size System_Numerics_BigInteger_TryConvertFromSaturating_TOther_REF_TOther_REF_System_Numerics_BigInteger_,.-System_Numerics_BigInteger_TryConvertFromSaturating_TOther_REF_TOther_REF_System_Numerics_BigInteger_
.Lme_133:
.text 0
	.balign 16
.Lm_134:
	.local System_Numerics_BigInteger_System_Numerics_INumberBase_System_Numerics_BigInteger_TryConvertFromTruncating_TOther_REF_TOther_REF_System_Numerics_BigInteger_
	.type System_Numerics_BigInteger_System_Numerics_INumberBase_System_Numerics_BigInteger_TryConvertFromTruncating_TOther_REF_TOther_REF_System_Numerics_BigInteger_,@function
System_Numerics_BigInteger_System_Numerics_INumberBase_System_Numerics_BigInteger_TryConvertFromTruncating_TOther_REF_TOther_REF_System_Numerics_BigInteger_:
.inst 0xa9ae7bfd
.inst 0x910003fd
.inst 0xf90013af
.inst 0xf9000ba0
.inst 0xf9000fa1

adrp x16, mono_aot_System_Runtime_Numerics_got+0
add x16, x16, :lo12:mono_aot_System_Runtime_Numerics_got
ldr x0, [x16, 56]
.inst 0xf9400011
.inst 0xb4000051
bl .Lp_1
.inst 0xf9400fa0
.inst 0xd2800001
.inst 0xf9000001
.inst 0xf9000401
.inst 0xd2a00000
.inst 0x53001c00
.inst 0x910003bf
.inst 0xa8d27bfd
.inst 0xd65f03c0

	.size System_Numerics_BigInteger_System_Numerics_INumberBase_System_Numerics_BigInteger_TryConvertFromTruncating_TOther_REF_TOther_REF_System_Numerics_BigInteger_,.-System_Numerics_BigInteger_System_Numerics_INumberBase_System_Numerics_BigInteger_TryConvertFromTruncating_TOther_REF_TOther_REF_System_Numerics_BigInteger_
.Lme_134:
.text 0
	.balign 16
.Lm_135:
	.local System_Numerics_BigInteger_TryConvertFromTruncating_TOther_REF_TOther_REF_System_Numerics_BigInteger_
	.type System_Numerics_BigInteger_TryConvertFromTruncating_TOther_REF_TOther_REF_System_Numerics_BigInteger_,@function
System_Numerics_BigInteger_TryConvertFromTruncating_TOther_REF_TOther_REF_System_Numerics_BigInteger_:
.inst 0xa9ae7bfd
.inst 0x910003fd
.inst 0xf90013af
.inst 0xf9000ba0
.inst 0xf9000fa1

adrp x16, mono_aot_System_Runtime_Numerics_got+0
add x16, x16, :lo12:mono_aot_System_Runtime_Numerics_got
ldr x0, [x16, 56]
.inst 0xf9400011
.inst 0xb4000051
bl .Lp_1
.inst 0xd2800001
.inst 0xf9400fa0
.inst 0xf9000001
.inst 0xf9000401
.inst 0xd2a00000
.inst 0x910003bf
.inst 0xa8d27bfd
.inst 0xd65f03c0

	.size System_Numerics_BigInteger_TryConvertFromTruncating_TOther_REF_TOther_REF_System_Numerics_BigInteger_,.-System_Numerics_BigInteger_TryConvertFromTruncating_TOther_REF_TOther_REF_System_Numerics_BigInteger_
.Lme_135:
.text 0
	.balign 16
.Lm_136:
	.local System_Numerics_BigInteger_System_Numerics_INumberBase_System_Numerics_BigInteger_TryConvertToChecked_TOther_REF_System_Numerics_BigInteger_TOther_REF_
	.type System_Numerics_BigInteger_System_Numerics_INumberBase_System_Numerics_BigInteger_TryConvertToChecked_TOther_REF_System_Numerics_BigInteger_TOther_REF_,@function
System_Numerics_BigInteger_System_Numerics_INumberBase_System_Numerics_BigInteger_TryConvertToChecked_TOther_REF_System_Numerics_BigInteger_TOther_REF_:
.inst 0xa9bc7bfd
.inst 0x910003fd
.inst 0xf90017af
.inst 0xa90107a0
.inst 0xf90013a2

adrp x16, mono_aot_System_Runtime_Numerics_got+0
add x16, x16, :lo12:mono_aot_System_Runtime_Numerics_got
ldr x0, [x16, 56]
.inst 0xf9400011
.inst 0xb4000051
bl .Lp_1

adrp x16, mono_aot_System_Runtime_Numerics_got+0
add x16, x16, :lo12:mono_aot_System_Runtime_Numerics_got
ldr x1, [x16, 304]
.inst 0xf94017a0
.inst 0xf9400c10
.inst 0xb5000050
bl .Lp_3
.inst 0xf94013a0
.inst 0xf900001f
.inst 0xd2a00000
.inst 0x910003bf
.inst 0xa8c47bfd
.inst 0xd65f03c0

	.size System_Numerics_BigInteger_System_Numerics_INumberBase_System_Numerics_BigInteger_TryConvertToChecked_TOther_REF_System_Numerics_BigInteger_TOther_REF_,.-System_Numerics_BigInteger_System_Numerics_INumberBase_System_Numerics_BigInteger_TryConvertToChecked_TOther_REF_System_Numerics_BigInteger_TOther_REF_
.Lme_136:
.text 0
	.balign 16
.Lm_137:
	.local System_Numerics_BigInteger_System_Numerics_INumberBase_System_Numerics_BigInteger_TryConvertToSaturating_TOther_REF_System_Numerics_BigInteger_TOther_REF_
	.type System_Numerics_BigInteger_System_Numerics_INumberBase_System_Numerics_BigInteger_TryConvertToSaturating_TOther_REF_System_Numerics_BigInteger_TOther_REF_,@function
System_Numerics_BigInteger_System_Numerics_INumberBase_System_Numerics_BigInteger_TryConvertToSaturating_TOther_REF_System_Numerics_BigInteger_TOther_REF_:
.inst 0xd2804a10
.inst 0x910003f1
.inst 0xcb100231
.inst 0x9100023f
.inst 0xa9007bfd
.inst 0x910003fd
.inst 0xf90017af
.inst 0xa90107a0
.inst 0xf90013a2

adrp x16, mono_aot_System_Runtime_Numerics_got+0
add x16, x16, :lo12:mono_aot_System_Runtime_Numerics_got
ldr x0, [x16, 56]
.inst 0xf9400011
.inst 0xb4000051
bl .Lp_1

adrp x16, mono_aot_System_Runtime_Numerics_got+0
add x16, x16, :lo12:mono_aot_System_Runtime_Numerics_got
ldr x1, [x16, 312]
.inst 0xf94017a0
.inst 0xf9400c10
.inst 0xb5000050
bl .Lp_3
.inst 0xf94013a0
.inst 0xf900001f
.inst 0xd2a00000
.inst 0x910003bf
.inst 0xa9407bfd
.inst 0xd2804a10
.inst 0x910003f1
.inst 0x8b100231
.inst 0x9100023f
.inst 0xd65f03c0

	.size System_Numerics_BigInteger_System_Numerics_INumberBase_System_Numerics_BigInteger_TryConvertToSaturating_TOther_REF_System_Numerics_BigInteger_TOther_REF_,.-System_Numerics_BigInteger_System_Numerics_INumberBase_System_Numerics_BigInteger_TryConvertToSaturating_TOther_REF_System_Numerics_BigInteger_TOther_REF_
.Lme_137:
.text 0
	.balign 16
.Lm_138:
	.local System_Numerics_BigInteger_System_Numerics_INumberBase_System_Numerics_BigInteger_TryConvertToTruncating_TOther_REF_System_Numerics_BigInteger_TOther_REF_
	.type System_Numerics_BigInteger_System_Numerics_INumberBase_System_Numerics_BigInteger_TryConvertToTruncating_TOther_REF_System_Numerics_BigInteger_TOther_REF_,@function
System_Numerics_BigInteger_System_Numerics_INumberBase_System_Numerics_BigInteger_TryConvertToTruncating_TOther_REF_System_Numerics_BigInteger_TOther_REF_:
.inst 0xa9a17bfd
.inst 0x910003fd
.inst 0xf90017af
.inst 0xa90107a0
.inst 0xf90013a2

adrp x16, mono_aot_System_Runtime_Numerics_got+0
add x16, x16, :lo12:mono_aot_System_Runtime_Numerics_got
ldr x0, [x16, 56]
.inst 0xf9400011
.inst 0xb4000051
bl .Lp_1

adrp x16, mono_aot_System_Runtime_Numerics_got+0
add x16, x16, :lo12:mono_aot_System_Runtime_Numerics_got
ldr x1, [x16, 320]
.inst 0xf94017a0
.inst 0xf9400c10
.inst 0xb5000050
bl .Lp_3
.inst 0xd2800000
.inst 0xf90023a0
.inst 0xf90027a0
.inst 0xd2800000
.inst 0xf9001ba0
.inst 0xf9001fa0
.inst 0xf94013a0
.inst 0xf900001f
.inst 0xd2a00000
.inst 0x910003bf
.inst 0xa8df7bfd
.inst 0xd65f03c0

	.size System_Numerics_BigInteger_System_Numerics_INumberBase_System_Numerics_BigInteger_TryConvertToTruncating_TOther_REF_System_Numerics_BigInteger_TOther_REF_,.-System_Numerics_BigInteger_System_Numerics_INumberBase_System_Numerics_BigInteger_TryConvertToTruncating_TOther_REF_System_Numerics_BigInteger_TOther_REF_
.Lme_138:
.text 0
	.balign 16
.Lm_16f:
	.local System_Numerics_Complex_CreateChecked_TOther_REF_TOther_REF
	.type System_Numerics_Complex_CreateChecked_TOther_REF_TOther_REF,@function
System_Numerics_Complex_CreateChecked_TOther_REF_TOther_REF:
.inst 0xa9b97bfd
.inst 0x910003fd
.inst 0xf9002faf
.inst 0xf9002ba0

adrp x16, mono_aot_System_Runtime_Numerics_got+0
add x16, x16, :lo12:mono_aot_System_Runtime_Numerics_got
ldr x0, [x16, 56]
.inst 0xf9400011
.inst 0xb4000051
bl .Lp_1

adrp x16, mono_aot_System_Runtime_Numerics_got+0
add x16, x16, :lo12:mono_aot_System_Runtime_Numerics_got
ldr x1, [x16, 328]
.inst 0xf9402fa0
.inst 0xf9400c10
.inst 0xb5000050
bl .Lp_3
.inst 0xd2800000
.inst 0xf90033a0
.inst 0xf90037a0
.inst 0xf9402fa0
.inst 0xf940100f
.inst 0xf9402ba0
.inst 0x910183a1
bl .Lp_18
.inst 0x53001c00
.inst 0x35000180
.inst 0xf9402fa0
.inst 0xf9401402
.inst 0xf9402ba0
.inst 0x910183a1
.inst 0xd63f0040
.inst 0x53001c00
.inst 0x350000a0
.inst 0xd2801e00
.inst 0xf2a04000
bl .Lp_16
bl .Lp_17
.inst 0xf94033a0
.inst 0xf9000ba0
.inst 0xf94037a0
.inst 0xf9000fa0
.inst 0xfd400ba0
.inst 0xfd400fa1
.inst 0x910003bf
.inst 0xa8c77bfd
.inst 0xd65f03c0

	.size System_Numerics_Complex_CreateChecked_TOther_REF_TOther_REF,.-System_Numerics_Complex_CreateChecked_TOther_REF_TOther_REF
.Lme_16f:
.text 0
	.balign 16
.Lm_170:
	.local System_Numerics_Complex_CreateSaturating_TOther_REF_TOther_REF
	.type System_Numerics_Complex_CreateSaturating_TOther_REF_TOther_REF,@function
System_Numerics_Complex_CreateSaturating_TOther_REF_TOther_REF:
.inst 0xa9b97bfd
.inst 0x910003fd
.inst 0xf9002faf
.inst 0xf9002ba0

adrp x16, mono_aot_System_Runtime_Numerics_got+0
add x16, x16, :lo12:mono_aot_System_Runtime_Numerics_got
ldr x0, [x16, 56]
.inst 0xf9400011
.inst 0xb4000051
bl .Lp_1

adrp x16, mono_aot_System_Runtime_Numerics_got+0
add x16, x16, :lo12:mono_aot_System_Runtime_Numerics_got
ldr x1, [x16, 336]
.inst 0xf9402fa0
.inst 0xf9400c10
.inst 0xb5000050
bl .Lp_3
.inst 0xd2800000
.inst 0xf90033a0
.inst 0xf90037a0
.inst 0xf9402fa0
.inst 0xf940100f
.inst 0xf9402ba0
.inst 0x910183a1
bl .Lp_19
.inst 0x53001c00
.inst 0x35000180
.inst 0xf9402fa0
.inst 0xf9401402
.inst 0xf9402ba0
.inst 0x910183a1
.inst 0xd63f0040
.inst 0x53001c00
.inst 0x350000a0
.inst 0xd2801e00
.inst 0xf2a04000
bl .Lp_16
bl .Lp_17
.inst 0xf94033a0
.inst 0xf9000ba0
.inst 0xf94037a0
.inst 0xf9000fa0
.inst 0xfd400ba0
.inst 0xfd400fa1
.inst 0x910003bf
.inst 0xa8c77bfd
.inst 0xd65f03c0

	.size System_Numerics_Complex_CreateSaturating_TOther_REF_TOther_REF,.-System_Numerics_Complex_CreateSaturating_TOther_REF_TOther_REF
.Lme_170:
.text 0
	.balign 16
.Lm_171:
	.local System_Numerics_Complex_CreateTruncating_TOther_REF_TOther_REF
	.type System_Numerics_Complex_CreateTruncating_TOther_REF_TOther_REF,@function
System_Numerics_Complex_CreateTruncating_TOther_REF_TOther_REF:
.inst 0xa9b97bfd
.inst 0x910003fd
.inst 0xf9002faf
.inst 0xf9002ba0

adrp x16, mono_aot_System_Runtime_Numerics_got+0
add x16, x16, :lo12:mono_aot_System_Runtime_Numerics_got
ldr x0, [x16, 56]
.inst 0xf9400011
.inst 0xb4000051
bl .Lp_1

adrp x16, mono_aot_System_Runtime_Numerics_got+0
add x16, x16, :lo12:mono_aot_System_Runtime_Numerics_got
ldr x1, [x16, 344]
.inst 0xf9402fa0
.inst 0xf9400c10
.inst 0xb5000050
bl .Lp_3
.inst 0xd2800000
.inst 0xf90033a0
.inst 0xf90037a0
.inst 0xf9402fa0
.inst 0xf940100f
.inst 0xf9402ba0
.inst 0x910183a1
bl .Lp_20
.inst 0x53001c00
.inst 0x35000180
.inst 0xf9402fa0
.inst 0xf9401402
.inst 0xf9402ba0
.inst 0x910183a1
.inst 0xd63f0040
.inst 0x53001c00
.inst 0x350000a0
.inst 0xd2801e00
.inst 0xf2a04000
bl .Lp_16
bl .Lp_17
.inst 0xf94033a0
.inst 0xf9000ba0
.inst 0xf94037a0
.inst 0xf9000fa0
.inst 0xfd400ba0
.inst 0xfd400fa1
.inst 0x910003bf
.inst 0xa8c77bfd
.inst 0xd65f03c0

	.size System_Numerics_Complex_CreateTruncating_TOther_REF_TOther_REF,.-System_Numerics_Complex_CreateTruncating_TOther_REF_TOther_REF
.Lme_171:
.text 0
	.balign 16
.Lm_174:
	.local System_Numerics_Complex_System_Numerics_INumberBase_System_Numerics_Complex_TryConvertFromChecked_TOther_REF_TOther_REF_System_Numerics_Complex_
	.type System_Numerics_Complex_System_Numerics_INumberBase_System_Numerics_Complex_TryConvertFromChecked_TOther_REF_TOther_REF_System_Numerics_Complex_,@function
System_Numerics_Complex_System_Numerics_INumberBase_System_Numerics_Complex_TryConvertFromChecked_TOther_REF_TOther_REF_System_Numerics_Complex_:
.inst 0xa9bd7bfd
.inst 0x910003fd
.inst 0xf90013af
.inst 0xf9000ba0
.inst 0xf9000fa1

adrp x16, mono_aot_System_Runtime_Numerics_got+0
add x16, x16, :lo12:mono_aot_System_Runtime_Numerics_got
ldr x0, [x16, 56]
.inst 0xf9400011
.inst 0xb4000051
bl .Lp_1

adrp x16, mono_aot_System_Runtime_Numerics_got+0
add x16, x16, :lo12:mono_aot_System_Runtime_Numerics_got
ldr x1, [x16, 352]
.inst 0xf94013a0
.inst 0xf9400c10
.inst 0xb5000050
bl .Lp_3
.inst 0xf94013a0
.inst 0xf940100f
.inst 0xf9400ba0
.inst 0xf9400fa1
bl .Lp_21
.inst 0x53001c00
.inst 0x910003bf
.inst 0xa8c37bfd
.inst 0xd65f03c0

	.size System_Numerics_Complex_System_Numerics_INumberBase_System_Numerics_Complex_TryConvertFromChecked_TOther_REF_TOther_REF_System_Numerics_Complex_,.-System_Numerics_Complex_System_Numerics_INumberBase_System_Numerics_Complex_TryConvertFromChecked_TOther_REF_TOther_REF_System_Numerics_Complex_
.Lme_174:
.text 0
	.balign 16
.Lm_175:
	.local System_Numerics_Complex_System_Numerics_INumberBase_System_Numerics_Complex_TryConvertFromSaturating_TOther_REF_TOther_REF_System_Numerics_Complex_
	.type System_Numerics_Complex_System_Numerics_INumberBase_System_Numerics_Complex_TryConvertFromSaturating_TOther_REF_TOther_REF_System_Numerics_Complex_,@function
System_Numerics_Complex_System_Numerics_INumberBase_System_Numerics_Complex_TryConvertFromSaturating_TOther_REF_TOther_REF_System_Numerics_Complex_:
.inst 0xa9bd7bfd
.inst 0x910003fd
.inst 0xf90013af
.inst 0xf9000ba0
.inst 0xf9000fa1

adrp x16, mono_aot_System_Runtime_Numerics_got+0
add x16, x16, :lo12:mono_aot_System_Runtime_Numerics_got
ldr x0, [x16, 56]
.inst 0xf9400011
.inst 0xb4000051
bl .Lp_1

adrp x16, mono_aot_System_Runtime_Numerics_got+0
add x16, x16, :lo12:mono_aot_System_Runtime_Numerics_got
ldr x1, [x16, 360]
.inst 0xf94013a0
.inst 0xf9400c10
.inst 0xb5000050
bl .Lp_3
.inst 0xf94013a0
.inst 0xf940100f
.inst 0xf9400ba0
.inst 0xf9400fa1
bl .Lp_22
.inst 0x53001c00
.inst 0x910003bf
.inst 0xa8c37bfd
.inst 0xd65f03c0

	.size System_Numerics_Complex_System_Numerics_INumberBase_System_Numerics_Complex_TryConvertFromSaturating_TOther_REF_TOther_REF_System_Numerics_Complex_,.-System_Numerics_Complex_System_Numerics_INumberBase_System_Numerics_Complex_TryConvertFromSaturating_TOther_REF_TOther_REF_System_Numerics_Complex_
.Lme_175:
.text 0
	.balign 16
.Lm_176:
	.local System_Numerics_Complex_System_Numerics_INumberBase_System_Numerics_Complex_TryConvertFromTruncating_TOther_REF_TOther_REF_System_Numerics_Complex_
	.type System_Numerics_Complex_System_Numerics_INumberBase_System_Numerics_Complex_TryConvertFromTruncating_TOther_REF_TOther_REF_System_Numerics_Complex_,@function
System_Numerics_Complex_System_Numerics_INumberBase_System_Numerics_Complex_TryConvertFromTruncating_TOther_REF_TOther_REF_System_Numerics_Complex_:
.inst 0xa9bd7bfd
.inst 0x910003fd
.inst 0xf90013af
.inst 0xf9000ba0
.inst 0xf9000fa1

adrp x16, mono_aot_System_Runtime_Numerics_got+0
add x16, x16, :lo12:mono_aot_System_Runtime_Numerics_got
ldr x0, [x16, 56]
.inst 0xf9400011
.inst 0xb4000051
bl .Lp_1

adrp x16, mono_aot_System_Runtime_Numerics_got+0
add x16, x16, :lo12:mono_aot_System_Runtime_Numerics_got
ldr x1, [x16, 368]
.inst 0xf94013a0
.inst 0xf9400c10
.inst 0xb5000050
bl .Lp_3
.inst 0xf94013a0
.inst 0xf940100f
.inst 0xf9400ba0
.inst 0xf9400fa1
bl .Lp_23
.inst 0x53001c00
.inst 0x910003bf
.inst 0xa8c37bfd
.inst 0xd65f03c0

	.size System_Numerics_Complex_System_Numerics_INumberBase_System_Numerics_Complex_TryConvertFromTruncating_TOther_REF_TOther_REF_System_Numerics_Complex_,.-System_Numerics_Complex_System_Numerics_INumberBase_System_Numerics_Complex_TryConvertFromTruncating_TOther_REF_TOther_REF_System_Numerics_Complex_
.Lme_176:
.text 0
	.balign 16
.Lm_177:
	.local System_Numerics_Complex_TryConvertFrom_TOther_REF_TOther_REF_System_Numerics_Complex_
	.type System_Numerics_Complex_TryConvertFrom_TOther_REF_TOther_REF_System_Numerics_Complex_,@function
System_Numerics_Complex_TryConvertFrom_TOther_REF_TOther_REF_System_Numerics_Complex_:
.inst 0xa9b07bfd
.inst 0x910003fd
.inst 0xf90013af
.inst 0xf9000ba0
.inst 0xf9000fa1

adrp x16, mono_aot_System_Runtime_Numerics_got+0
add x16, x16, :lo12:mono_aot_System_Runtime_Numerics_got
ldr x0, [x16, 56]
.inst 0xf9400011
.inst 0xb4000051
bl .Lp_1
.inst 0xd2800001
.inst 0xf9400fa0
.inst 0xf9000001
.inst 0xf9000401
.inst 0xd2a00000
.inst 0x910003bf
.inst 0xa8d07bfd
.inst 0xd65f03c0

	.size System_Numerics_Complex_TryConvertFrom_TOther_REF_TOther_REF_System_Numerics_Complex_,.-System_Numerics_Complex_TryConvertFrom_TOther_REF_TOther_REF_System_Numerics_Complex_
.Lme_177:
.text 0
	.balign 16
.Lm_178:
	.local System_Numerics_Complex_System_Numerics_INumberBase_System_Numerics_Complex_TryConvertToChecked_TOther_REF_System_Numerics_Complex_TOther_REF_
	.type System_Numerics_Complex_System_Numerics_INumberBase_System_Numerics_Complex_TryConvertToChecked_TOther_REF_System_Numerics_Complex_TOther_REF_,@function
System_Numerics_Complex_System_Numerics_INumberBase_System_Numerics_Complex_TryConvertToChecked_TOther_REF_System_Numerics_Complex_TOther_REF_:
.inst 0xa9b77bfd
.inst 0x910003fd
.inst 0xf9002faf
.inst 0xfd000ba0
.inst 0xfd000fa1
.inst 0xf9002ba0

adrp x16, mono_aot_System_Runtime_Numerics_got+0
add x16, x16, :lo12:mono_aot_System_Runtime_Numerics_got
ldr x0, [x16, 56]
.inst 0xf9400011
.inst 0xb4000051
bl .Lp_1

adrp x16, mono_aot_System_Runtime_Numerics_got+0
add x16, x16, :lo12:mono_aot_System_Runtime_Numerics_got
ldr x1, [x16, 376]
.inst 0xf9402fa0
.inst 0xf9400c10
.inst 0xb5000050
bl .Lp_3
.inst 0xf9402ba0
.inst 0xf900001f
.inst 0xd2a00000
.inst 0x910003bf
.inst 0xa8c97bfd
.inst 0xd65f03c0

	.size System_Numerics_Complex_System_Numerics_INumberBase_System_Numerics_Complex_TryConvertToChecked_TOther_REF_System_Numerics_Complex_TOther_REF_,.-System_Numerics_Complex_System_Numerics_INumberBase_System_Numerics_Complex_TryConvertToChecked_TOther_REF_System_Numerics_Complex_TOther_REF_
.Lme_178:
.text 0
	.balign 16
.Lm_179:
	.local System_Numerics_Complex_System_Numerics_INumberBase_System_Numerics_Complex_TryConvertToSaturating_TOther_REF_System_Numerics_Complex_TOther_REF_
	.type System_Numerics_Complex_System_Numerics_INumberBase_System_Numerics_Complex_TryConvertToSaturating_TOther_REF_System_Numerics_Complex_TOther_REF_,@function
System_Numerics_Complex_System_Numerics_INumberBase_System_Numerics_Complex_TryConvertToSaturating_TOther_REF_System_Numerics_Complex_TOther_REF_:
.inst 0xa9b17bfd
.inst 0x910003fd
.inst 0xf9002faf
.inst 0xfd000ba0
.inst 0xfd000fa1
.inst 0xf9002ba0

adrp x16, mono_aot_System_Runtime_Numerics_got+0
add x16, x16, :lo12:mono_aot_System_Runtime_Numerics_got
ldr x0, [x16, 56]
.inst 0xf9400011
.inst 0xb4000051
bl .Lp_1

adrp x16, mono_aot_System_Runtime_Numerics_got+0
add x16, x16, :lo12:mono_aot_System_Runtime_Numerics_got
ldr x1, [x16, 384]
.inst 0xf9402fa0
.inst 0xf9400c10
.inst 0xb5000050
bl .Lp_3
.inst 0xf9402ba0
.inst 0xf900001f
.inst 0xd2a00000
.inst 0x910003bf
.inst 0xa8cf7bfd
.inst 0xd65f03c0

	.size System_Numerics_Complex_System_Numerics_INumberBase_System_Numerics_Complex_TryConvertToSaturating_TOther_REF_System_Numerics_Complex_TOther_REF_,.-System_Numerics_Complex_System_Numerics_INumberBase_System_Numerics_Complex_TryConvertToSaturating_TOther_REF_System_Numerics_Complex_TOther_REF_
.Lme_179:
.text 0
	.balign 16
.Lm_17a:
	.local System_Numerics_Complex_System_Numerics_INumberBase_System_Numerics_Complex_TryConvertToTruncating_TOther_REF_System_Numerics_Complex_TOther_REF_
	.type System_Numerics_Complex_System_Numerics_INumberBase_System_Numerics_Complex_TryConvertToTruncating_TOther_REF_System_Numerics_Complex_TOther_REF_,@function
System_Numerics_Complex_System_Numerics_INumberBase_System_Numerics_Complex_TryConvertToTruncating_TOther_REF_System_Numerics_Complex_TOther_REF_:
.inst 0xa9b17bfd
.inst 0x910003fd
.inst 0xf9002faf
.inst 0xfd000ba0
.inst 0xfd000fa1
.inst 0xf9002ba0

adrp x16, mono_aot_System_Runtime_Numerics_got+0
add x16, x16, :lo12:mono_aot_System_Runtime_Numerics_got
ldr x0, [x16, 56]
.inst 0xf9400011
.inst 0xb4000051
bl .Lp_1

adrp x16, mono_aot_System_Runtime_Numerics_got+0
add x16, x16, :lo12:mono_aot_System_Runtime_Numerics_got
ldr x1, [x16, 392]
.inst 0xf9402fa0
.inst 0xf9400c10
.inst 0xb5000050
bl .Lp_3
.inst 0xf9402ba0
.inst 0xf900001f
.inst 0xd2a00000
.inst 0x910003bf
.inst 0xa8cf7bfd
.inst 0xd65f03c0

	.size System_Numerics_Complex_System_Numerics_INumberBase_System_Numerics_Complex_TryConvertToTruncating_TOther_REF_System_Numerics_Complex_TOther_REF_,.-System_Numerics_Complex_System_Numerics_INumberBase_System_Numerics_Complex_TryConvertToTruncating_TOther_REF_System_Numerics_Complex_TOther_REF_
.Lme_17a:
.text 0
	.balign 8
jit_code_end:

	.byte 0,0,0,0
.section ".data.rel.ro"
.subsection 0
	.balign 8
method_addresses:
	.local method_addresses
	.type method_addresses,@object
bl method_addresses
bl method_addresses
bl method_addresses
bl method_addresses
bl method_addresses
bl method_addresses
bl method_addresses
bl method_addresses
bl method_addresses
bl method_addresses
bl method_addresses
bl method_addresses
bl method_addresses
bl method_addresses
bl method_addresses
bl method_addresses
bl method_addresses
bl method_addresses
bl method_addresses
bl method_addresses
bl method_addresses
bl method_addresses
bl method_addresses
bl method_addresses
bl method_addresses
bl method_addresses
bl method_addresses
bl method_addresses
bl method_addresses
bl method_addresses
bl method_addresses
bl method_addresses
bl method_addresses
bl method_addresses
bl method_addresses
bl method_addresses
bl method_addresses
bl method_addresses
bl method_addresses
bl method_addresses
bl method_addresses
bl method_addresses
bl method_addresses
bl method_addresses
bl method_addresses
bl method_addresses
bl method_addresses
bl method_addresses
bl method_addresses
bl method_addresses
bl method_addresses
bl method_addresses
bl method_addresses
bl method_addresses
bl method_addresses
bl method_addresses
bl method_addresses
bl method_addresses
bl method_addresses
bl method_addresses
bl method_addresses
bl method_addresses
bl method_addresses
bl method_addresses
bl method_addresses
bl method_addresses
bl method_addresses
bl method_addresses
bl method_addresses
bl method_addresses
bl method_addresses
bl method_addresses
bl method_addresses
bl method_addresses
bl method_addresses
bl method_addresses
bl method_addresses
bl method_addresses
bl method_addresses
bl method_addresses
bl method_addresses
bl method_addresses
bl method_addresses
bl method_addresses
bl method_addresses
bl method_addresses
bl method_addresses
bl method_addresses
bl method_addresses
bl method_addresses
bl method_addresses
bl method_addresses
bl method_addresses
bl method_addresses
bl method_addresses
bl method_addresses
bl method_addresses
bl method_addresses
bl method_addresses
bl method_addresses
bl method_addresses
bl method_addresses
bl method_addresses
bl method_addresses
bl method_addresses
bl method_addresses
bl method_addresses
bl method_addresses
bl method_addresses
bl method_addresses
bl method_addresses
bl method_addresses
bl method_addresses
bl method_addresses
bl method_addresses
bl method_addresses
bl method_addresses
bl method_addresses
bl method_addresses
bl method_addresses
bl .Lm_78
bl .Lm_79
bl .Lm_7a
bl .Lm_7b
bl .Lm_7c
bl .Lm_7d
bl .Lm_7e
bl .Lm_7f
bl .Lm_80
bl .Lm_81
bl .Lm_82
bl .Lm_83
bl .Lm_84
bl method_addresses
bl method_addresses
bl method_addresses
bl method_addresses
bl method_addresses
bl method_addresses
bl method_addresses
bl method_addresses
bl method_addresses
bl method_addresses
bl method_addresses
bl method_addresses
bl method_addresses
bl method_addresses
bl method_addresses
bl method_addresses
bl method_addresses
bl method_addresses
bl method_addresses
bl method_addresses
bl method_addresses
bl method_addresses
bl method_addresses
bl method_addresses
bl method_addresses
bl method_addresses
bl method_addresses
bl method_addresses
bl method_addresses
bl method_addresses
bl method_addresses
bl method_addresses
bl method_addresses
bl method_addresses
bl method_addresses
bl method_addresses
bl method_addresses
bl method_addresses
bl method_addresses
bl method_addresses
bl method_addresses
bl method_addresses
bl method_addresses
bl method_addresses
bl method_addresses
bl method_addresses
bl method_addresses
bl method_addresses
bl method_addresses
bl method_addresses
bl method_addresses
bl method_addresses
bl method_addresses
bl method_addresses
bl method_addresses
bl method_addresses
bl method_addresses
bl method_addresses
bl method_addresses
bl method_addresses
bl method_addresses
bl method_addresses
bl method_addresses
bl method_addresses
bl method_addresses
bl method_addresses
bl method_addresses
bl method_addresses
bl method_addresses
bl method_addresses
bl method_addresses
bl method_addresses
bl method_addresses
bl method_addresses
bl method_addresses
bl method_addresses
bl method_addresses
bl method_addresses
bl method_addresses
bl method_addresses
bl method_addresses
bl method_addresses
bl method_addresses
bl method_addresses
bl method_addresses
bl method_addresses
bl method_addresses
bl method_addresses
bl method_addresses
bl method_addresses
bl method_addresses
bl method_addresses
bl method_addresses
bl method_addresses
bl method_addresses
bl method_addresses
bl method_addresses
bl method_addresses
bl method_addresses
bl method_addresses
bl method_addresses
bl method_addresses
bl method_addresses
bl method_addresses
bl method_addresses
bl method_addresses
bl method_addresses
bl method_addresses
bl method_addresses
bl method_addresses
bl method_addresses
bl method_addresses
bl method_addresses
bl method_addresses
bl method_addresses
bl method_addresses
bl method_addresses
bl method_addresses
bl method_addresses
bl method_addresses
bl method_addresses
bl method_addresses
bl method_addresses
bl method_addresses
bl method_addresses
bl method_addresses
bl method_addresses
bl method_addresses
bl method_addresses
bl method_addresses
bl method_addresses
bl method_addresses
bl method_addresses
bl method_addresses
bl method_addresses
bl method_addresses
bl method_addresses
bl method_addresses
bl method_addresses
bl method_addresses
bl method_addresses
bl method_addresses
bl method_addresses
bl method_addresses
bl method_addresses
bl method_addresses
bl method_addresses
bl method_addresses
bl method_addresses
bl method_addresses
bl method_addresses
bl method_addresses
bl method_addresses
bl method_addresses
bl method_addresses
bl method_addresses
bl method_addresses
bl method_addresses
bl method_addresses
bl method_addresses
bl method_addresses
bl method_addresses
bl method_addresses
bl method_addresses
bl .Lm_129
bl .Lm_12a
bl .Lm_12b
bl method_addresses
bl method_addresses
bl method_addresses
bl method_addresses
bl .Lm_130
bl .Lm_131
bl .Lm_132
bl .Lm_133
bl .Lm_134
bl .Lm_135
bl .Lm_136
bl .Lm_137
bl .Lm_138
bl method_addresses
bl method_addresses
bl method_addresses
bl method_addresses
bl method_addresses
bl method_addresses
bl method_addresses
bl method_addresses
bl method_addresses
bl method_addresses
bl method_addresses
bl method_addresses
bl method_addresses
bl method_addresses
bl method_addresses
bl method_addresses
bl method_addresses
bl method_addresses
bl method_addresses
bl method_addresses
bl method_addresses
bl method_addresses
bl method_addresses
bl method_addresses
bl method_addresses
bl method_addresses
bl method_addresses
bl method_addresses
bl method_addresses
bl method_addresses
bl method_addresses
bl method_addresses
bl method_addresses
bl method_addresses
bl method_addresses
bl method_addresses
bl method_addresses
bl method_addresses
bl method_addresses
bl method_addresses
bl method_addresses
bl method_addresses
bl method_addresses
bl method_addresses
bl method_addresses
bl method_addresses
bl method_addresses
bl method_addresses
bl method_addresses
bl method_addresses
bl method_addresses
bl method_addresses
bl method_addresses
bl method_addresses
bl .Lm_16f
bl .Lm_170
bl .Lm_171
bl method_addresses
bl method_addresses
bl .Lm_174
bl .Lm_175
bl .Lm_176
bl .Lm_177
bl .Lm_178
bl .Lm_179
bl .Lm_17a
bl method_addresses
bl method_addresses
bl method_addresses
bl method_addresses
bl method_addresses
bl method_addresses
method_addresses_end:

.text 0
	.balign 8
unbox_trampolines:
unbox_trampolines_end:

	.long 0
.text 0
	.balign 8
unbox_trampoline_addresses:

	.long 0
.text 0
	.balign 8
method_info_offsets:

	.byte 129,1,0,0,10,0,0,0,39,0,0,0,2,0,0,0,0,0,10,0,20,0,30,0,40,0,50,0,60,0,70,0
	.byte 80,0,90,0,100,0,110,0,120,0,130,0,144,0,154,0,164,0,174,0,184,0,194,0,204,0,214,0,224,0,234,0
	.byte 244,0,254,0,8,1,18,1,28,1,38,1,48,1,58,1,73,1,83,1,93,1,103,1,113,1,124,1,139,1,0,0
	.byte 0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0
	.byte 0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0
	.byte 0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0
	.byte 0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,1,4,4,6,6,6,6,6,6,6
	.byte 55,6,6,255,255,255,255,189,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0
	.byte 0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0
	.byte 0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0
	.byte 0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0
	.byte 0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0
	.byte 0,0,0,0,0,0,0,0,0,0,0,73,8,8,0,0,0,0,97,6,6,6,6,6,128,133,8,8,255,255,255,255
	.byte 107,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0
	.byte 0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,128,157,8,8,0,0,128,181,8,8
	.byte 8,6,8,8,255,255,255,255,29,0,0,0,0,0
.text 0
	.balign 8
method_flags_table:

	.byte 0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0
	.byte 0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0
	.byte 0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0
	.byte 0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,4,4,4,4,4,4
	.byte 4,0,4,4,4,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0
	.byte 0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0
	.byte 0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0
	.byte 0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0
	.byte 0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0
	.byte 0,0,0,0,0,0,0,0,0,5,5,5,0,0,0,0,1,1,1,1,1,1,5,5,5,0,0,0,0,0,0,0
	.byte 0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0
	.byte 0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,5,5,5,0,0,5,5,5,1,5,5,5,0,0,0,0,0
	.byte 0
.text 0
	.balign 8
extra_method_table:

	.byte 11,0,0,0,11,0,0,0,4,0,0,0,2,0,0,0,2,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0
	.byte 0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0
	.byte 0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0
	.byte 0,0,0,0,0,0,0,0,0,0,0,0
.text 0
	.balign 8
extra_method_info_offsets:

	.byte 0,0,0,0
.text 0
	.balign 8
class_name_table:

	.byte 73,0,22,0,0,0,8,0,0,0,0,0,0,0,0,0,0,0,11,0,0,0,27,0,0,0,0,0,0,0,1,0
	.byte 0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,25,0,0,0,0,0,0,0,0,0,0,0,16,0
	.byte 0,0,19,0,0,0,0,0,0,0,0,0,0,0,7,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0
	.byte 0,0,20,0,0,0,2,0,73,0,24,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,26,0,0,0,0,0
	.byte 0,0,0,0,0,0,0,0,0,0,5,0,74,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0
	.byte 0,0,4,0,0,0,9,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0
	.byte 0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,3,0,0,0,0,0,0,0,10,0
	.byte 0,0,18,0,0,0,0,0,0,0,0,0,0,0,13,0,0,0,12,0,75,0,0,0,0,0,0,0,0,0,23,0
	.byte 0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,17,0
	.byte 0,0,21,0,0,0,6,0,0,0,14,0,0,0,15,0,0,0
.text 0
	.balign 8
got_info_offsets:

	.byte 50,0,0,0,10,0,0,0,5,0,0,0,2,0,0,0,0,0,11,0,22,0,33,0,44,0,128,235,2,1,1,1
	.byte 1,1,1,1,1,128,247,2,2,2,2,3,2,2,2,2,129,12,3,2,3,3,26,30,29,34,34,129,206,31,38,38
	.byte 35,69,60,60,53,37,131,152,47,74,74,67,47,47,47,37,37
.text 0
	.balign 8
ex_info_offsets:

	.byte 129,1,0,0,10,0,0,0,39,0,0,0,2,0,0,0,0,0,10,0,20,0,30,0,40,0,50,0,60,0,70,0
	.byte 80,0,90,0,100,0,110,0,120,0,134,0,149,0,159,0,169,0,179,0,189,0,199,0,209,0,219,0,229,0,239,0
	.byte 249,0,3,1,13,1,23,1,33,1,43,1,54,1,65,1,80,1,90,1,100,1,110,1,120,1,131,1,146,1,0,0
	.byte 0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0
	.byte 0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0
	.byte 0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0
	.byte 0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,134,95,60,46,127,128,159,128,148,128
	.byte 166,110,100,113,138,169,110,125,255,255,255,244,108,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0
	.byte 0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0
	.byte 0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0
	.byte 0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0
	.byte 0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0
	.byte 0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,140,207,90,90,0,0,0,0,141,239,65,73,65,73,65,143
	.byte 123,52,53,255,255,255,240,28,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0
	.byte 0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,144,24,96
	.byte 96,0,0,145,56,52,52,70,55,52,52,255,255,255,237,123,0,0,0,0,0
.text 1
	.balign 8
unwind_info:

	.byte 16,12,31,0,68,14,48,157,6,158,5,68,13,29,68,154,4,13,12,31,0,68,14,32,157,4,158,3,68,13,29,18
	.byte 12,31,0,68,14,64,157,8,158,7,68,13,29,68,152,6,153,5,18,12,31,0,68,14,112,157,14,158,13,68,13,29
	.byte 68,153,12,154,11,24,12,31,0,68,14,144,1,157,18,158,17,68,13,29,68,151,16,152,15,68,153,14,154,13,24,12
	.byte 31,0,68,14,224,1,157,28,158,27,68,13,29,68,151,26,152,25,68,153,24,154,23,23,12,31,0,68,14,112,157,14
	.byte 158,13,68,13,29,68,151,12,152,11,68,153,10,154,9,21,12,31,0,68,14,112,157,14,158,13,68,13,29,68,151,12
	.byte 152,11,68,153,10,16,12,31,0,68,14,64,157,8,158,7,68,13,29,68,153,6,21,12,31,0,68,14,112,157,14,158
	.byte 13,68,13,29,68,152,12,153,11,68,154,10,23,12,31,0,68,14,112,157,14,158,13,68,13,29,68,150,12,151,11,68
	.byte 152,10,153,9,16,12,31,0,68,14,48,157,6,158,5,68,13,29,68,153,4,27,12,31,0,68,14,176,1,157,22,158
	.byte 21,68,13,29,68,150,20,151,19,68,152,18,153,17,68,154,16,14,12,31,0,68,14,176,2,157,38,158,37,68,13,29
	.byte 14,12,31,0,68,14,144,2,157,34,158,33,68,13,29,14,12,31,0,68,14,160,2,157,36,158,35,68,13,29,13,12
	.byte 31,0,68,14,64,157,8,158,7,68,13,29,14,12,31,0,84,14,208,4,157,74,158,73,68,13,29,14,12,31,0,68
	.byte 14,240,3,157,62,158,61,68,13,29,13,12,31,0,68,14,112,157,14,158,13,68,13,29,13,12,31,0,68,14,48,157
	.byte 6,158,5,68,13,29,14,12,31,0,68,14,128,2,157,32,158,31,68,13,29,14,12,31,0,68,14,144,1,157,18,158
	.byte 17,68,13,29,14,12,31,0,68,14,240,1,157,30,158,29,68,13,29
.text 0
	.balign 8
class_info_offsets:

	.byte 27,0,0,0,10,0,0,0,3,0,0,0,2,0,0,0,0,0,11,0,22,0,146,185,7,23,24,23,20,103,103,24
	.byte 5,148,10,5,5,28,28,20,5,23,23,5,148,255,23,5,24,23,24,24

.text 0
	.balign 16
plt:
mono_aot_System_Runtime_Numerics_plt:
	.local plt__jit_icall_mono_threads_state_poll
	.type plt__jit_icall_mono_threads_state_poll,@function
plt__jit_icall_mono_threads_state_poll:
.Lp_1:
adrp x16, mono_aot_System_Runtime_Numerics_got+0
add x16, x16, :lo12:mono_aot_System_Runtime_Numerics_got
ldr x16, [x16, 408]
br x16
.inst 1416
	.size plt__jit_icall_mono_threads_state_poll,.-plt__jit_icall_mono_threads_state_poll
	.local plt__jit_icall_mono_arch_throw_corlib_exception
	.type plt__jit_icall_mono_arch_throw_corlib_exception,@function
plt__jit_icall_mono_arch_throw_corlib_exception:
.Lp_2:
adrp x16, mono_aot_System_Runtime_Numerics_got+0
add x16, x16, :lo12:mono_aot_System_Runtime_Numerics_got
ldr x16, [x16, 416]
br x16
.inst 1419
	.size plt__jit_icall_mono_arch_throw_corlib_exception,.-plt__jit_icall_mono_arch_throw_corlib_exception
	.local plt__jit_icall_mini_init_method_rgctx
	.type plt__jit_icall_mini_init_method_rgctx,@function
plt__jit_icall_mini_init_method_rgctx:
.Lp_3:
adrp x16, mono_aot_System_Runtime_Numerics_got+0
add x16, x16, :lo12:mono_aot_System_Runtime_Numerics_got
ldr x16, [x16, 424]
br x16
.inst 1421
	.size plt__jit_icall_mini_init_method_rgctx,.-plt__jit_icall_mini_init_method_rgctx
	.local plt_System_Collections_Generic_ValueListBuilder_1_T_REF_AddWithResize_T_REF
	.type plt_System_Collections_Generic_ValueListBuilder_1_T_REF_AddWithResize_T_REF,@function
plt_System_Collections_Generic_ValueListBuilder_1_T_REF_AddWithResize_T_REF:
.Lp_4:
adrp x16, mono_aot_System_Runtime_Numerics_got+0
add x16, x16, :lo12:mono_aot_System_Runtime_Numerics_got
ldr x16, [x16, 432]
br x16
.inst 1424
	.size plt_System_Collections_Generic_ValueListBuilder_1_T_REF_AddWithResize_T_REF,.-plt_System_Collections_Generic_ValueListBuilder_1_T_REF_AddWithResize_T_REF
	.local plt_System_Collections_Generic_ValueListBuilder_1_T_REF_AppendMultiChar_System_ReadOnlySpan_1_T_REF
	.type plt_System_Collections_Generic_ValueListBuilder_1_T_REF_AppendMultiChar_System_ReadOnlySpan_1_T_REF,@function
plt_System_Collections_Generic_ValueListBuilder_1_T_REF_AppendMultiChar_System_ReadOnlySpan_1_T_REF:
.Lp_5:
adrp x16, mono_aot_System_Runtime_Numerics_got+0
add x16, x16, :lo12:mono_aot_System_Runtime_Numerics_got
ldr x16, [x16, 440]
br x16
.inst 1439
	.size plt_System_Collections_Generic_ValueListBuilder_1_T_REF_AppendMultiChar_System_ReadOnlySpan_1_T_REF,.-plt_System_Collections_Generic_ValueListBuilder_1_T_REF_AppendMultiChar_System_ReadOnlySpan_1_T_REF
	.local plt_System_Collections_Generic_ValueListBuilder_1_T_REF_Grow_int
	.type plt_System_Collections_Generic_ValueListBuilder_1_T_REF_Grow_int,@function
plt_System_Collections_Generic_ValueListBuilder_1_T_REF_Grow_int:
.Lp_6:
adrp x16, mono_aot_System_Runtime_Numerics_got+0
add x16, x16, :lo12:mono_aot_System_Runtime_Numerics_got
ldr x16, [x16, 448]
br x16
.inst 1453
	.size plt_System_Collections_Generic_ValueListBuilder_1_T_REF_Grow_int,.-plt_System_Collections_Generic_ValueListBuilder_1_T_REF_Grow_int
	.local plt_System_Buffer_BulkMoveWithWriteBarrier_byte__byte__uintptr_intptr
	.type plt_System_Buffer_BulkMoveWithWriteBarrier_byte__byte__uintptr_intptr,@function
plt_System_Buffer_BulkMoveWithWriteBarrier_byte__byte__uintptr_intptr:
.Lp_7:
adrp x16, mono_aot_System_Runtime_Numerics_got+0
add x16, x16, :lo12:mono_aot_System_Runtime_Numerics_got
ldr x16, [x16, 456]
br x16
.inst 1468
	.size plt_System_Buffer_BulkMoveWithWriteBarrier_byte__byte__uintptr_intptr,.-plt_System_Buffer_BulkMoveWithWriteBarrier_byte__byte__uintptr_intptr
	.local plt_System_ThrowHelper_ThrowArgumentException_DestinationTooShort
	.type plt_System_ThrowHelper_ThrowArgumentException_DestinationTooShort,@function
plt_System_ThrowHelper_ThrowArgumentException_DestinationTooShort:
.Lp_8:
adrp x16, mono_aot_System_Runtime_Numerics_got+0
add x16, x16, :lo12:mono_aot_System_Runtime_Numerics_got
ldr x16, [x16, 464]
br x16
.inst 1473
	.size plt_System_ThrowHelper_ThrowArgumentException_DestinationTooShort,.-plt_System_ThrowHelper_ThrowArgumentException_DestinationTooShort
	.local plt_System_ThrowHelper_ThrowArgumentOutOfRangeException
	.type plt_System_ThrowHelper_ThrowArgumentOutOfRangeException,@function
plt_System_ThrowHelper_ThrowArgumentOutOfRangeException:
.Lp_9:
adrp x16, mono_aot_System_Runtime_Numerics_got+0
add x16, x16, :lo12:mono_aot_System_Runtime_Numerics_got
ldr x16, [x16, 472]
br x16
.inst 1478
	.size plt_System_ThrowHelper_ThrowArgumentOutOfRangeException,.-plt_System_ThrowHelper_ThrowArgumentOutOfRangeException
	.local plt_System_Collections_Generic_ValueListBuilder_1_T_REF_AppendSpanWithGrow_int
	.type plt_System_Collections_Generic_ValueListBuilder_1_T_REF_AppendSpanWithGrow_int,@function
plt_System_Collections_Generic_ValueListBuilder_1_T_REF_AppendSpanWithGrow_int:
.Lp_10:
adrp x16, mono_aot_System_Runtime_Numerics_got+0
add x16, x16, :lo12:mono_aot_System_Runtime_Numerics_got
ldr x16, [x16, 480]
br x16
.inst 1483
	.size plt_System_Collections_Generic_ValueListBuilder_1_T_REF_AppendSpanWithGrow_int,.-plt_System_Collections_Generic_ValueListBuilder_1_T_REF_AppendSpanWithGrow_int
	.local plt_System_Span_1_T_REF_TryCopyTo_System_Span_1_T_REF
	.type plt_System_Span_1_T_REF_TryCopyTo_System_Span_1_T_REF,@function
plt_System_Span_1_T_REF_TryCopyTo_System_Span_1_T_REF:
.Lp_11:
adrp x16, mono_aot_System_Runtime_Numerics_got+0
add x16, x16, :lo12:mono_aot_System_Runtime_Numerics_got
ldr x16, [x16, 488]
br x16
.inst 1498
	.size plt_System_Span_1_T_REF_TryCopyTo_System_Span_1_T_REF,.-plt_System_Span_1_T_REF_TryCopyTo_System_Span_1_T_REF
	.local plt_System_Type_get_IsPrimitive
	.type plt_System_Type_get_IsPrimitive,@function
plt_System_Type_get_IsPrimitive:
.Lp_12:
adrp x16, mono_aot_System_Runtime_Numerics_got+0
add x16, x16, :lo12:mono_aot_System_Runtime_Numerics_got
ldr x16, [x16, 496]
br x16
.inst 1513
	.size plt_System_Type_get_IsPrimitive,.-plt_System_Type_get_IsPrimitive
	.local plt_System_Array_Clear_System_Array_int_int
	.type plt_System_Array_Clear_System_Array_int_int,@function
plt_System_Array_Clear_System_Array_int_int:
.Lp_13:
adrp x16, mono_aot_System_Runtime_Numerics_got+0
add x16, x16, :lo12:mono_aot_System_Runtime_Numerics_got
ldr x16, [x16, 504]
br x16
.inst 1518
	.size plt_System_Array_Clear_System_Array_int_int,.-plt_System_Array_Clear_System_Array_int_int
	.local plt_System_Buffers_ArrayPool_1_T_REF_get_Shared
	.type plt_System_Buffers_ArrayPool_1_T_REF_get_Shared,@function
plt_System_Buffers_ArrayPool_1_T_REF_get_Shared:
.Lp_14:
adrp x16, mono_aot_System_Runtime_Numerics_got+0
add x16, x16, :lo12:mono_aot_System_Runtime_Numerics_got
ldr x16, [x16, 512]
br x16
.inst 1523
	.size plt_System_Buffers_ArrayPool_1_T_REF_get_Shared,.-plt_System_Buffers_ArrayPool_1_T_REF_get_Shared
	.local plt_System_ThrowHelper_ThrowArrayTypeMismatchException
	.type plt_System_ThrowHelper_ThrowArrayTypeMismatchException,@function
plt_System_ThrowHelper_ThrowArrayTypeMismatchException:
.Lp_15:
adrp x16, mono_aot_System_Runtime_Numerics_got+0
add x16, x16, :lo12:mono_aot_System_Runtime_Numerics_got
ldr x16, [x16, 520]
br x16
.inst 1538
	.size plt_System_ThrowHelper_ThrowArrayTypeMismatchException,.-plt_System_ThrowHelper_ThrowArrayTypeMismatchException
	.local plt__jit_icall_mono_create_corlib_exception_0
	.type plt__jit_icall_mono_create_corlib_exception_0,@function
plt__jit_icall_mono_create_corlib_exception_0:
.Lp_16:
adrp x16, mono_aot_System_Runtime_Numerics_got+0
add x16, x16, :lo12:mono_aot_System_Runtime_Numerics_got
ldr x16, [x16, 528]
br x16
.inst 1543
	.size plt__jit_icall_mono_create_corlib_exception_0,.-plt__jit_icall_mono_create_corlib_exception_0
	.local plt__jit_icall_mono_arch_throw_exception
	.type plt__jit_icall_mono_arch_throw_exception,@function
plt__jit_icall_mono_arch_throw_exception:
.Lp_17:
adrp x16, mono_aot_System_Runtime_Numerics_got+0
add x16, x16, :lo12:mono_aot_System_Runtime_Numerics_got
ldr x16, [x16, 536]
br x16
.inst 1545
	.size plt__jit_icall_mono_arch_throw_exception,.-plt__jit_icall_mono_arch_throw_exception
	.local plt_System_Numerics_Complex_TryConvertFrom_TOther_REF_TOther_REF_System_Numerics_Complex_
	.type plt_System_Numerics_Complex_TryConvertFrom_TOther_REF_TOther_REF_System_Numerics_Complex_,@function
plt_System_Numerics_Complex_TryConvertFrom_TOther_REF_TOther_REF_System_Numerics_Complex_:
.Lp_18:
adrp x16, mono_aot_System_Runtime_Numerics_got+0
add x16, x16, :lo12:mono_aot_System_Runtime_Numerics_got
ldr x16, [x16, 544]
br x16
.inst 1547
	.size plt_System_Numerics_Complex_TryConvertFrom_TOther_REF_TOther_REF_System_Numerics_Complex_,.-plt_System_Numerics_Complex_TryConvertFrom_TOther_REF_TOther_REF_System_Numerics_Complex_
	.local plt_System_Numerics_Complex_TryConvertFrom_TOther_REF_TOther_REF_System_Numerics_Complex__0
	.type plt_System_Numerics_Complex_TryConvertFrom_TOther_REF_TOther_REF_System_Numerics_Complex__0,@function
plt_System_Numerics_Complex_TryConvertFrom_TOther_REF_TOther_REF_System_Numerics_Complex__0:
.Lp_19:
adrp x16, mono_aot_System_Runtime_Numerics_got+0
add x16, x16, :lo12:mono_aot_System_Runtime_Numerics_got
ldr x16, [x16, 552]
br x16
.inst 1561
	.size plt_System_Numerics_Complex_TryConvertFrom_TOther_REF_TOther_REF_System_Numerics_Complex__0,.-plt_System_Numerics_Complex_TryConvertFrom_TOther_REF_TOther_REF_System_Numerics_Complex__0
	.local plt_System_Numerics_Complex_TryConvertFrom_TOther_REF_TOther_REF_System_Numerics_Complex__1
	.type plt_System_Numerics_Complex_TryConvertFrom_TOther_REF_TOther_REF_System_Numerics_Complex__1,@function
plt_System_Numerics_Complex_TryConvertFrom_TOther_REF_TOther_REF_System_Numerics_Complex__1:
.Lp_20:
adrp x16, mono_aot_System_Runtime_Numerics_got+0
add x16, x16, :lo12:mono_aot_System_Runtime_Numerics_got
ldr x16, [x16, 560]
br x16
.inst 1575
	.size plt_System_Numerics_Complex_TryConvertFrom_TOther_REF_TOther_REF_System_Numerics_Complex__1,.-plt_System_Numerics_Complex_TryConvertFrom_TOther_REF_TOther_REF_System_Numerics_Complex__1
	.local plt_System_Numerics_Complex_TryConvertFrom_TOther_REF_TOther_REF_System_Numerics_Complex__2
	.type plt_System_Numerics_Complex_TryConvertFrom_TOther_REF_TOther_REF_System_Numerics_Complex__2,@function
plt_System_Numerics_Complex_TryConvertFrom_TOther_REF_TOther_REF_System_Numerics_Complex__2:
.Lp_21:
adrp x16, mono_aot_System_Runtime_Numerics_got+0
add x16, x16, :lo12:mono_aot_System_Runtime_Numerics_got
ldr x16, [x16, 568]
br x16
.inst 1589
	.size plt_System_Numerics_Complex_TryConvertFrom_TOther_REF_TOther_REF_System_Numerics_Complex__2,.-plt_System_Numerics_Complex_TryConvertFrom_TOther_REF_TOther_REF_System_Numerics_Complex__2
	.local plt_System_Numerics_Complex_TryConvertFrom_TOther_REF_TOther_REF_System_Numerics_Complex__3
	.type plt_System_Numerics_Complex_TryConvertFrom_TOther_REF_TOther_REF_System_Numerics_Complex__3,@function
plt_System_Numerics_Complex_TryConvertFrom_TOther_REF_TOther_REF_System_Numerics_Complex__3:
.Lp_22:
adrp x16, mono_aot_System_Runtime_Numerics_got+0
add x16, x16, :lo12:mono_aot_System_Runtime_Numerics_got
ldr x16, [x16, 576]
br x16
.inst 1603
	.size plt_System_Numerics_Complex_TryConvertFrom_TOther_REF_TOther_REF_System_Numerics_Complex__3,.-plt_System_Numerics_Complex_TryConvertFrom_TOther_REF_TOther_REF_System_Numerics_Complex__3
	.local plt_System_Numerics_Complex_TryConvertFrom_TOther_REF_TOther_REF_System_Numerics_Complex__4
	.type plt_System_Numerics_Complex_TryConvertFrom_TOther_REF_TOther_REF_System_Numerics_Complex__4,@function
plt_System_Numerics_Complex_TryConvertFrom_TOther_REF_TOther_REF_System_Numerics_Complex__4:
.Lp_23:
adrp x16, mono_aot_System_Runtime_Numerics_got+0
add x16, x16, :lo12:mono_aot_System_Runtime_Numerics_got
ldr x16, [x16, 584]
br x16
.inst 1617
	.size plt_System_Numerics_Complex_TryConvertFrom_TOther_REF_TOther_REF_System_Numerics_Complex__4,.-plt_System_Numerics_Complex_TryConvertFrom_TOther_REF_TOther_REF_System_Numerics_Complex__4
	.size mono_aot_System_Runtime_Numerics_plt,.-mono_aot_System_Runtime_Numerics_plt
plt_end:
.text 0
	.balign 8
image_table:

	.byte 2,0,0,0,83,121,115,116,101,109,46,82,117,110,116,105,109,101,46,78,117,109,101,114,105,99,115,0,67,65,52,51
	.byte 65,50,55,66,45,52,52,70,54,45,52,69,67,68,45,57,66,50,57,45,68,67,55,56,65,51,66,51,56,56,51,70
	.byte 0,0,98,48,51,102,53,102,55,102,49,49,100,53,48,97,51,97,0,0,0,0,0,0,1,0,0,0,10,0,0,0
	.byte 0,0,0,0,0,0,0,0,0,0,0,0,83,121,115,116,101,109,46,80,114,105,118,97,116,101,46,67,111,114,101,76
	.byte 105,98,0,48,55,56,65,54,65,68,48,45,70,65,65,55,45,52,54,48,50,45,66,68,66,66,45,52,67,56,51,55
	.byte 48,49,55,68,49,52,67,0,0,55,99,101,99,56,53,100,55,98,101,97,55,55,57,56,101,0,0,0,0,0,0,0
	.byte 1,0,0,0,10,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0
.text 0
	.balign 8
weak_field_indexes:

	.byte 0,0,0,0
.section ".bss"
.subsection 0
	.balign 8
	.local mono_aot_System_Runtime_Numerics_got
	.type mono_aot_System_Runtime_Numerics_got,@object
mono_aot_System_Runtime_Numerics_got:
	.skip 592
got_end:
.text 0
	.balign 8
blob:

	.byte 0,120,0,0,0,121,0,0,0,122,0,0,0,1,25,123,0,0,0,1,26,124,0,0,0,1,27,125,0,0,0,1
	.byte 28,126,0,0,0,1,29,127,0,0,0,1,30,128,0,0,0,1,31,129,0,0,0,130,0,0,0,1,32,131,0,0
	.byte 0,1,33,132,0,0,0,1,34,41,1,0,0,1,19,1,35,42,1,0,0,1,19,1,36,43,1,0,0,1,19,1
	.byte 37,48,1,0,0,1,19,49,1,0,0,1,19,50,1,0,0,1,19,51,1,0,0,1,19,52,1,0,0,1,19,53
	.byte 1,0,0,1,19,54,1,0,0,1,19,1,38,55,1,0,0,1,19,1,39,56,1,0,0,1,19,1,40,111,1,0
	.byte 0,1,22,1,41,112,1,0,0,1,22,1,42,113,1,0,0,1,22,1,43,116,1,0,0,1,22,1,44,117,1,0
	.byte 0,1,22,1,45,118,1,0,0,1,22,1,46,119,1,0,0,1,22,120,1,0,0,1,22,1,47,121,1,0,0,1
	.byte 22,1,48,122,1,0,0,1,22,1,49,11,0,36,38,45,50,52,32,47,48,55,8,55,9,55,10,55,11,55,12,55
	.byte 128,243,6,80,6,89,6,91,6,92,6,96,6,128,249,6,83,6,128,165,6,128,142,6,128,141,5,0,19,0,1,0
	.byte 1,16,5,1,28,7,129,26,1,7,129,34,4,1,16,129,40,67,255,253,0,0,0,7,129,44,0,123,1,129,40,1
	.byte 10,255,253,0,0,0,7,129,44,0,128,129,1,129,40,67,255,253,0,0,0,7,129,44,0,124,1,129,40,1,10,255
	.byte 253,0,0,0,7,129,44,0,125,1,129,40,67,255,253,0,0,0,7,129,44,0,125,1,129,40,2,10,255,253,0,0
	.byte 0,7,129,44,0,128,133,1,129,40,5,7,129,34,67,255,253,0,0,0,7,129,44,0,126,1,129,40,2,10,255,253
	.byte 0,0,0,7,129,44,0,128,133,1,129,40,5,7,129,34,67,255,253,0,0,0,7,129,44,0,127,1,129,40,1,10
	.byte 255,253,0,0,0,7,129,44,0,128,128,1,129,40,67,255,253,0,0,0,7,129,44,0,128,128,1,129,40,1,10,255
	.byte 253,0,0,0,7,129,44,0,128,133,1,129,40,67,255,253,0,0,0,7,129,44,0,128,129,1,129,40,1,10,255,253
	.byte 0,0,0,7,129,44,0,128,133,1,129,40,4,2,129,26,1,129,40,67,255,253,0,0,0,7,129,44,0,128,131,1
	.byte 129,40,1,10,255,253,0,0,0,7,130,12,1,146,196,1,129,40,4,2,130,63,1,129,40,67,255,253,0,0,0,7
	.byte 129,44,0,128,132,1,129,40,2,10,255,253,0,0,0,7,130,50,1,160,31,1,129,40,5,7,129,34,67,255,253,0
	.byte 0,0,7,129,44,0,128,133,1,129,40,3,10,255,253,0,0,0,7,130,50,1,160,31,1,129,40,5,6,1,7,129
	.byte 34,5,7,129,34,5,0,30,0,1,1,129,42,5,1,28,7,130,133,1,7,130,141,4,2,129,218,1,130,147,1,1
	.byte 19,67,255,253,0,0,0,1,19,0,129,42,2,130,147,1,29,7,130,141,255,253,0,0,0,7,130,151,1,156,76,3
	.byte 130,147,130,158,5,0,30,0,1,1,129,43,5,1,28,7,130,196,1,7,130,204,4,2,129,218,1,130,210,67,255,253
	.byte 0,0,0,1,19,0,129,43,2,130,210,1,29,7,130,204,255,253,0,0,0,7,130,214,1,156,77,3,130,210,130,158
	.byte 5,0,30,0,1,1,129,44,5,1,28,7,131,0,1,7,131,8,4,2,129,218,1,131,14,67,255,253,0,0,0,1
	.byte 19,0,129,44,2,131,14,1,29,7,131,8,255,253,0,0,0,7,131,18,1,156,78,3,131,14,130,158,5,0,30,0
	.byte 1,1,129,55,5,1,28,7,131,60,1,7,131,68,67,255,253,0,0,0,1,19,0,129,55,2,131,74,1,14,7,131
	.byte 68,5,0,30,0,1,1,129,56,5,1,28,7,131,97,1,7,131,105,67,255,253,0,0,0,1,19,0,129,56,2,131
	.byte 111,1,14,7,131,105,5,0,30,0,1,1,129,57,5,1,28,7,131,134,1,7,131,142,67,255,253,0,0,0,1,19
	.byte 0,129,57,2,131,148,1,14,7,131,142,5,0,30,0,1,1,129,112,5,1,28,7,131,171,1,7,131,179,4,2,129
	.byte 218,1,131,185,1,1,22,67,255,253,0,0,0,1,22,0,129,112,2,131,185,2,10,255,253,0,0,0,1,22,0,129
	.byte 120,2,131,185,29,7,131,179,255,253,0,0,0,7,131,189,1,156,76,3,131,185,131,196,5,0,30,0,1,1,129,113
	.byte 5,1,28,7,131,248,1,7,132,0,4,2,129,218,1,132,6,67,255,253,0,0,0,1,22,0,129,113,2,132,6,2
	.byte 10,255,253,0,0,0,1,22,0,129,120,2,132,6,29,7,132,0,255,253,0,0,0,7,132,10,1,156,77,3,132,6
	.byte 131,196,5,0,30,0,1,1,129,114,5,1,28,7,132,66,1,7,132,74,4,2,129,218,1,132,80,67,255,253,0,0
	.byte 0,1,22,0,129,114,2,132,80,2,10,255,253,0,0,0,1,22,0,129,120,2,132,80,29,7,132,74,255,253,0,0
	.byte 0,7,132,84,1,156,78,3,132,80,131,196,5,0,30,0,1,1,129,117,5,1,28,7,132,140,1,7,132,148,67,255
	.byte 253,0,0,0,1,22,0,129,117,2,132,154,1,10,255,253,0,0,0,1,22,0,129,120,2,132,154,5,0,30,0,1
	.byte 1,129,118,5,1,28,7,132,187,1,7,132,195,67,255,253,0,0,0,1,22,0,129,118,2,132,201,1,10,255,253,0
	.byte 0,0,1,22,0,129,120,2,132,201,5,0,30,0,1,1,129,119,5,1,28,7,132,234,1,7,132,242,67,255,253,0
	.byte 0,0,1,22,0,129,119,2,132,248,1,10,255,253,0,0,0,1,22,0,129,120,2,132,248,5,0,30,0,1,1,129
	.byte 121,5,1,28,7,133,25,1,7,133,33,67,255,253,0,0,0,1,22,0,129,121,2,133,39,1,14,7,133,33,5,0
	.byte 30,0,1,1,129,122,5,1,28,7,133,62,1,7,133,70,67,255,253,0,0,0,1,22,0,129,122,2,133,76,1,14
	.byte 7,133,70,5,0,30,0,1,1,129,123,5,1,28,7,133,99,1,7,133,107,67,255,253,0,0,0,1,22,0,129,123
	.byte 2,133,113,1,14,7,133,107,6,128,249,6,103,6,129,10,3,255,253,0,0,0,7,129,44,0,128,129,1,129,40,3
	.byte 255,253,0,0,0,7,129,44,0,125,1,129,40,3,255,253,0,0,0,7,129,44,0,128,133,1,129,40,3,193,0,1
	.byte 126,3,193,0,19,139,3,193,0,19,138,3,255,253,0,0,0,7,129,44,0,128,128,1,129,40,3,255,253,0,0,0
	.byte 7,130,12,1,146,196,1,129,40,3,193,0,5,42,3,193,0,0,228,3,255,253,0,0,0,7,130,50,1,160,31,1
	.byte 129,40,3,193,0,19,135,6,127,6,104,3,255,253,0,0,0,1,22,0,129,120,2,131,185,3,255,253,0,0,0,1
	.byte 22,0,129,120,2,132,6,3,255,253,0,0,0,1,22,0,129,120,2,132,80,3,255,253,0,0,0,1,22,0,129,120
	.byte 2,132,154,3,255,253,0,0,0,1,22,0,129,120,2,132,201,3,255,253,0,0,0,1,22,0,129,120,2,132,248,11
	.byte 0,2,1,15,16,0,29,40,16,0,13,255,253,0,0,0,7,129,44,0,121,1,129,40,0,0,8,0,96,14,8,14
	.byte 8,14,56,23,84,48,112,0,9,0,48,2,0,5,4,2,0,5,4,2,0,0,4,5,24,1,0,11,17,2,1,15
	.byte 12,0,29,24,12,0,13,255,253,0,0,0,7,129,44,0,122,1,129,40,0,0,4,0,80,12,16,13,48,40,60,0
	.byte 4,0,40,1,4,5,4,1,0,11,31,2,1,15,16,0,29,40,16,0,13,255,253,0,0,0,7,129,44,0,123,1
	.byte 129,40,0,0,21,0,176,1,14,8,14,32,16,8,4,16,16,64,12,88,18,16,2,8,14,40,77,128,228,88,129,0
	.byte 0,35,0,88,1,0,6,4,7,16,3,4,5,0,0,4,2,4,3,0,0,4,0,4,0,4,0,4,0,8,0,4
	.byte 0,4,6,0,0,8,0,4,0,4,0,4,0,12,0,4,0,8,5,0,3,0,1,4,5,4,0,0,1,4,2,4
	.byte 0,4,0,8,5,4,1,0,11,50,2,1,15,16,0,29,64,16,0,13,255,253,0,0,0,7,129,44,0,124,1,129
	.byte 40,0,0,27,0,176,1,14,8,14,32,14,8,6,24,16,8,4,16,16,64,16,64,20,104,18,16,2,8,14,80,103
	.byte 129,48,88,129,76,0,48,0,88,1,0,6,4,7,16,2,4,6,0,0,8,2,4,3,4,5,0,0,4,2,4,3
	.byte 0,0,4,0,4,0,4,0,4,0,8,0,4,0,4,8,0,0,4,0,4,0,4,0,4,0,8,0,4,0,4,5
	.byte 0,0,8,0,8,0,4,0,4,0,4,0,12,0,4,0,8,10,0,3,0,1,4,5,4,0,0,1,4,2,20,0
	.byte 4,0,12,5,4,1,0,11,69,2,1,15,20,0,29,120,20,0,13,255,253,0,0,0,7,129,44,0,125,1,129,40
	.byte 0,0,27,0,160,1,26,16,24,40,4,16,24,32,28,24,12,40,0,0,38,200,1,10,160,2,28,16,12,16,92,129
	.byte 168,80,129,204,0,38,0,80,1,0,7,8,5,0,2,4,0,4,5,12,5,0,0,4,2,4,2,0,0,4,5,12
	.byte 6,0,5,4,3,8,5,0,1,8,0,4,0,4,0,0,0,4,5,0,0,0,3,4,0,4,6,8,255,255,255,255
	.byte 250,4,6,4,5,76,255,255,255,255,242,0,24,128,144,2,0,7,8,5,0,1,4,5,4,1,0,11,94,2,1,15
	.byte 20,0,29,128,184,20,0,13,255,253,0,0,0,7,129,44,0,126,1,129,40,0,0,33,0,192,1,26,16,24,40,4
	.byte 16,16,8,10,32,0,0,38,216,1,30,40,10,160,1,10,160,2,26,192,2,28,16,12,16,103,130,168,96,130,208,0
	.byte 41,0,96,1,0,7,8,5,0,2,4,0,4,5,12,5,0,0,4,2,4,3,4,5,4,0,4,0,4,0,0,0
	.byte 4,5,0,1,0,0,4,7,8,255,255,255,255,249,4,5,4,2,4,11,84,0,0,3,4,0,4,7,12,255,255,255
	.byte 255,249,0,7,4,5,76,255,255,255,255,241,0,25,128,144,0,16,13,128,144,2,0,7,8,5,0,1,4,5,4,1
	.byte 0,11,119,2,1,15,20,0,29,80,20,0,13,255,253,0,0,0,7,129,44,0,127,1,129,40,0,0,20,0,184,1
	.byte 14,8,14,32,24,32,6,24,18,16,18,184,1,2,8,14,72,61,129,24,92,129,52,0,27,0,92,1,0,6,4,7
	.byte 16,1,0,2,4,1,4,3,8,5,0,1,4,0,4,2,4,3,0,1,4,5,4,0,0,2,4,1,4,6,84,0
	.byte 0,1,4,2,4,0,12,0,4,0,4,5,12,1,0,11,128,143,2,1,15,20,0,29,80,20,0,14,255,253,0,0
	.byte 0,7,129,44,0,128,128,1,129,40,0,0,14,0,160,1,14,8,24,32,18,72,28,32,26,208,1,55,129,0,80,129
	.byte 40,0,24,0,80,1,0,6,4,2,0,0,4,5,12,6,4,2,8,1,8,0,4,0,4,0,0,5,8,2,0,6
	.byte 8,1,4,5,4,1,0,0,4,0,8,5,4,1,4,6,84,1,0,11,128,165,2,1,15,16,0,29,32,16,0,14
	.byte 255,253,0,0,0,7,129,44,0,128,129,1,129,40,0,0,13,0,152,1,14,16,14,48,24,96,12,96,18,16,69,128
	.byte 212,76,128,252,0,31,0,76,1,0,6,8,2,4,0,4,0,4,0,4,5,8,1,0,0,4,6,8,0,8,0,4
	.byte 0,4,0,4,0,8,0,4,0,4,6,0,0,12,0,4,0,4,0,4,0,12,0,4,0,8,5,0,3,0,1,4
	.byte 5,4,1,0,11,128,182,2,1,15,20,0,29,104,20,0,14,255,253,0,0,0,7,129,44,0,128,130,1,129,40,0
	.byte 0,7,0,96,36,216,1,10,88,31,128,200,48,128,240,0,10,0,48,1,0,0,4,7,8,255,255,255,255,249,4,5
	.byte 4,2,4,5,84,10,44,1,0,11,128,204,2,1,15,20,0,29,88,20,0,14,255,253,0,0,0,7,129,44,0,128
	.byte 131,1,129,40,0,0,16,0,192,1,38,216,1,16,56,4,8,18,24,2,16,8,16,63,129,12,96,129,48,0,26,0
	.byte 96,1,0,0,4,7,8,255,255,255,255,249,4,5,4,2,4,11,84,3,4,0,4,0,12,0,4,0,4,5,0,0
	.byte 0,2,4,2,0,5,8,2,4,0,0,0,4,1,4,2,4,2,4,0,0,1,4,11,128,228,2,1,15,16,0,29
	.byte 32,16,0,14,255,253,0,0,0,7,129,44,0,128,132,1,129,40,0,0,21,0,144,1,14,16,6,16,14,16,30,32
	.byte 4,8,26,40,0,0,10,32,14,48,73,128,176,72,128,192,0,33,0,72,1,4,6,4,0,0,1,4,0,0,2,4
	.byte 2,4,5,4,10,8,0,4,0,4,5,0,0,0,2,4,3,4,5,4,0,4,0,4,0,0,0,4,5,0,0,4
	.byte 0,4,0,4,0,4,7,0,0,4,0,4,0,4,0,4,0,8,6,0,11,128,245,2,1,15,24,0,29,128,144,24
	.byte 0,14,255,253,0,0,0,7,129,44,0,128,133,1,129,40,0,0,60,0,168,1,22,32,4,8,6,16,22,32,26,40
	.byte 14,56,2,8,16,32,22,32,24,72,22,32,10,48,2,8,10,32,14,48,24,240,1,10,160,2,14,8,32,208,2,10
	.byte 56,6,8,30,32,4,8,26,32,0,0,10,32,14,48,128,222,131,108,84,131,152,0,101,0,84,1,0,0,4,5,12
	.byte 5,0,0,0,2,4,0,0,1,4,2,4,1,0,0,4,5,12,6,0,2,4,0,4,5,12,6,0,255,255,255,255
	.byte 244,4,12,4,1,20,5,0,1,4,6,0,0,12,2,4,1,0,0,4,5,12,6,0,1,4,5,32,6,0,0,4
	.byte 5,12,255,255,255,255,245,0,11,4,5,20,5,0,1,4,0,4,0,4,0,4,0,4,6,0,0,4,0,4,0,4
	.byte 0,8,6,4,1,0,0,4,0,8,6,108,255,255,255,255,250,0,16,128,144,1,0,6,4,0,0,4,4,1,4,0
	.byte 8,0,8,0,4,0,4,0,4,0,12,0,4,0,8,5,0,1,108,5,0,0,4,5,24,1,0,0,0,2,4,10
	.byte 8,0,4,0,4,5,0,0,0,2,4,3,0,5,4,0,4,0,4,0,0,0,4,5,0,0,4,0,4,0,4,0
	.byte 4,7,0,0,4,0,4,0,4,0,4,5,8,1,0,11,129,17,2,1,15,12,0,29,40,12,0,13,255,253,0,0
	.byte 0,1,19,0,129,42,2,130,147,0,0,17,82,160,1,16,40,4,8,28,48,4,8,10,32,0,0,2,32,43,128,164
	.byte 80,128,180,0,18,41,80,3,16,0,4,5,0,0,0,2,4,9,4,0,8,0,4,0,4,0,4,5,0,0,0,2
	.byte 4,0,16,5,0,1,16,1,0,11,129,17,2,1,15,12,0,29,40,12,0,13,255,253,0,0,0,1,19,0,129,43
	.byte 2,130,210,0,0,17,82,160,1,16,40,4,8,28,48,4,8,10,32,0,0,2,32,43,128,164,80,128,180,0,18,41
	.byte 80,3,16,0,4,5,0,0,0,2,4,9,4,0,8,0,4,0,4,0,4,5,0,0,0,2,4,0,16,5,0,1
	.byte 16,1,0,11,129,17,2,1,15,12,0,29,40,12,0,13,255,253,0,0,0,1,19,0,129,44,2,131,14,0,0,17
	.byte 82,160,1,16,40,4,8,28,48,4,8,10,32,0,0,2,32,43,128,164,80,128,180,0,18,41,80,3,16,0,4,5
	.byte 0,0,0,2,4,9,4,0,8,0,4,0,4,0,4,5,0,0,0,2,4,0,16,5,0,1,16,1,0,5,0,30
	.byte 0,1,1,129,49,5,1,28,7,141,221,1,7,141,229,11,129,32,2,1,15,12,0,29,32,12,0,13,255,253,0,0
	.byte 0,1,19,0,129,49,2,141,235,0,0,4,0,88,14,48,13,68,44,80,0,4,0,44,2,20,5,4,1,0,5,0
	.byte 30,0,1,1,129,50,5,1,28,7,142,30,1,7,142,38,11,129,32,2,1,15,12,0,29,32,12,0,13,255,253,0
	.byte 0,0,1,19,0,129,50,2,142,44,0,0,5,190,14,88,16,32,20,64,44,76,0,7,131,159,44,1,0,0,8,0
	.byte 4,7,4,0,0,1,4,5,0,30,0,1,1,129,51,5,1,28,7,142,103,1,7,142,111,11,129,47,2,1,15,12
	.byte 0,29,32,12,0,13,255,253,0,0,0,1,19,0,129,51,2,142,117,0,0,4,0,88,14,48,13,68,44,80,0,4
	.byte 0,44,2,20,5,4,1,0,5,0,30,0,1,1,129,52,5,1,28,7,142,168,1,7,142,176,11,129,47,2,1,15
	.byte 12,0,29,32,12,0,13,255,253,0,0,0,1,19,0,129,52,2,142,182,0,0,5,156,15,88,16,32,20,64,44,76
	.byte 0,7,131,206,44,1,0,0,8,0,4,7,4,0,0,1,4,5,0,30,0,1,1,129,53,5,1,28,7,142,241,1
	.byte 7,142,249,11,129,47,2,1,15,12,0,29,32,12,0,13,255,253,0,0,0,1,19,0,129,53,2,142,255,0,0,4
	.byte 0,88,14,48,13,68,44,80,0,4,0,44,2,20,5,4,1,0,5,0,30,0,1,1,129,54,5,1,28,7,143,50
	.byte 1,7,143,58,11,129,47,2,1,15,12,0,29,32,12,0,13,255,253,0,0,0,1,19,0,129,54,2,143,64,0,0
	.byte 5,156,15,88,16,32,20,64,44,76,0,7,131,206,44,1,0,0,8,0,4,7,4,0,0,1,4,11,129,62,2,1
	.byte 15,12,0,29,40,12,0,13,255,253,0,0,0,1,19,0,129,55,2,131,74,0,0,6,176,15,144,1,16,16,16,84
	.byte 72,96,0,5,131,216,72,1,4,7,4,0,0,1,4,11,129,76,2,1,15,28,0,29,40,28,0,13,255,253,0,0
	.byte 0,1,19,0,129,56,2,131,111,0,0,6,228,26,176,1,16,16,17,100,88,128,128,0,5,134,178,88,1,4,7,4
	.byte 0,0,1,4,11,129,91,2,1,15,12,0,29,40,12,0,13,255,253,0,0,0,1,19,0,129,57,2,131,148,0,0
	.byte 6,222,31,192,1,16,16,16,108,96,120,0,5,135,239,96,1,4,7,4,0,0,1,4,11,129,106,2,1,15,12,0
	.byte 29,88,12,0,13,255,253,0,0,0,1,22,0,129,112,2,131,185,0,0,17,82,160,1,16,48,4,8,28,48,4,8
	.byte 10,32,0,0,2,32,49,128,168,80,128,188,0,21,41,80,3,4,0,8,0,4,0,4,0,4,5,0,0,0,2,4
	.byte 9,4,0,8,0,4,0,4,0,4,5,0,0,0,2,4,0,16,5,0,1,16,1,0,11,129,106,2,1,15,12,0
	.byte 29,88,12,0,13,255,253,0,0,0,1,22,0,129,113,2,132,6,0,0,17,82,160,1,16,48,4,8,28,48,4,8
	.byte 10,32,0,0,2,32,49,128,168,80,128,188,0,21,41,80,3,4,0,8,0,4,0,4,0,4,5,0,0,0,2,4
	.byte 9,4,0,8,0,4,0,4,0,4,5,0,0,0,2,4,0,16,5,0,1,16,1,0,11,129,106,2,1,15,12,0
	.byte 29,88,12,0,13,255,253,0,0,0,1,22,0,129,114,2,132,80,0,0,17,82,160,1,16,48,4,8,28,48,4,8
	.byte 10,32,0,0,2,32,49,128,168,80,128,188,0,21,41,80,3,4,0,8,0,4,0,4,0,4,5,0,0,0,2,4
	.byte 9,4,0,8,0,4,0,4,0,4,5,0,0,0,2,4,0,16,5,0,1,16,1,0,11,129,120,2,1,15,12,0
	.byte 29,32,12,0,13,255,253,0,0,0,1,22,0,129,117,2,132,154,0,0,5,0,144,1,14,48,17,96,72,108,0,6
	.byte 0,72,2,4,0,12,0,4,5,4,1,0,11,129,120,2,1,15,12,0,29,32,12,0,13,255,253,0,0,0,1,22
	.byte 0,129,118,2,132,201,0,0,5,0,144,1,14,48,17,96,72,108,0,6,0,72,2,4,0,12,0,4,5,4,1,0
	.byte 11,129,120,2,1,15,12,0,29,32,12,0,13,255,253,0,0,0,1,22,0,129,119,2,132,248,0,0,5,0,144,1
	.byte 14,48,17,96,72,108,0,6,0,72,2,4,0,12,0,4,5,4,1,0,5,0,30,0,1,1,129,120,5,1,28,7
	.byte 145,212,1,7,145,220,11,129,134,2,1,15,12,0,29,32,12,0,13,255,253,0,0,0,1,22,0,129,120,2,145,226
	.byte 0,0,5,190,14,88,16,32,20,64,44,76,0,7,131,159,44,1,0,0,8,0,4,7,4,0,0,1,4,11,129,149
	.byte 2,1,15,12,0,29,88,12,0,13,255,253,0,0,0,1,22,0,129,121,2,133,39,0,0,6,162,22,152,1,16,16
	.byte 16,88,76,100,0,5,133,145,76,1,4,7,4,0,0,1,4,11,129,164,2,1,15,12,0,29,88,12,0,13,255,253
	.byte 0,0,0,1,22,0,129,122,2,133,76,0,0,6,134,26,152,1,16,16,16,88,76,100,0,5,134,131,76,1,4,7
	.byte 4,0,0,1,4,11,129,164,2,1,15,12,0,29,88,12,0,13,255,253,0,0,0,1,22,0,129,123,2,133,113,0
	.byte 0,6,134,26,152,1,16,16,16,88,76,100,0,5,134,131,76,1,4,7,4,0,0,1,4,0,128,144,16,0,0,1
	.byte 4,128,144,16,0,0,1,193,0,2,95,193,0,2,92,193,0,2,91,193,0,2,90,4,128,204,56,16,40,0,1,193
	.byte 0,2,95,193,0,2,92,193,0,2,91,193,0,2,90,4,128,128,32,0,0,8,193,0,5,161,193,0,5,160,193,0
	.byte 5,162,193,0,2,90,4,128,128,48,0,0,8,193,0,5,161,193,0,5,160,75,193,0,2,90,24,128,144,17,0,0
	.byte 1,193,0,1,208,193,0,1,207,193,0,1,210,193,0,2,90,193,0,1,209,193,0,1,221,193,0,1,213,193,0,1
	.byte 236,193,0,1,237,193,0,1,238,193,0,1,239,193,0,1,240,193,0,1,241,193,0,1,242,193,0,1,243,193,0,1
	.byte 244,193,0,1,245,193,0,1,246,193,0,1,247,193,0,1,248,193,0,1,249,193,0,1,250,193,0,1,212,193,0,1
	.byte 251,24,128,144,20,0,0,4,193,0,1,208,193,0,1,207,193,0,1,210,193,0,2,90,193,0,1,209,193,0,1,221
	.byte 193,0,1,213,193,0,1,236,193,0,1,237,193,0,1,238,193,0,1,239,193,0,1,240,193,0,1,241,193,0,1,242
	.byte 193,0,1,243,193,0,1,244,193,0,1,245,193,0,1,246,193,0,1,247,193,0,1,248,193,0,1,249,193,0,1,250
	.byte 193,0,1,212,193,0,1,251,4,128,196,76,16,16,0,1,193,0,2,95,193,0,2,92,193,0,2,91,193,0,2,90
	.byte 255,255,255,255,255,255,255,255,255,255,255,255,255,255,255,255,255,255,255,255,9,128,128,18,0,0,2,193,0,5,161,193
	.byte 0,5,160,193,0,5,162,193,0,2,90,98,99,100,101,102,9,128,144,17,0,0,1,193,0,5,161,193,0,5,160,193
	.byte 0,5,162,193,0,2,90,104,105,106,107,108,4,128,160,48,0,0,8,193,0,5,161,193,0,5,160,112,193,0,2,90
	.byte 255,255,255,255,255,4,128,144,16,0,0,1,193,0,2,95,193,0,2,92,193,0,2,91,193,0,2,90,4,128,144,16
	.byte 0,0,1,193,0,2,95,193,0,2,92,193,0,2,91,193,0,2,90,255,255,255,255,255,24,128,144,20,0,0,4,193
	.byte 0,1,208,193,0,1,207,193,0,1,210,193,0,2,90,193,0,1,209,193,0,1,221,193,0,1,213,193,0,1,236,193
	.byte 0,1,237,193,0,1,238,193,0,1,239,193,0,1,240,193,0,1,241,193,0,1,242,193,0,1,243,193,0,1,244,193
	.byte 0,1,245,193,0,1,246,193,0,1,247,193,0,1,248,193,0,1,249,193,0,1,250,193,0,1,212,193,0,1,251,4
	.byte 128,144,16,0,0,1,193,0,2,95,193,0,2,92,193,0,2,91,193,0,2,90,255,255,255,255,255,4,128,136,16,130
	.byte 88,0,1,193,0,2,95,193,0,2,92,193,0,2,91,193,0,2,90,4,128,144,56,0,4,4,193,0,5,161,193,0
	.byte 5,160,193,0,5,162,193,0,2,90,4,128,144,128,144,0,4,4,193,0,5,161,193,0,5,160,193,0,5,162,193,0
	.byte 2,90,4,128,144,128,192,0,4,4,193,0,5,161,193,0,5,160,193,0,5,162,193,0,2,90,4,128,144,129,16,0
	.byte 8,8,193,0,5,161,193,0,5,160,193,0,5,162,193,0,2,90,115,103,101,110,0
.text 1
runtime_version:
	.string ""
.text 1
assembly_guid:
	.string "CA43A27B-44F6-4ECD-9B29-DC78A3B3883F"
.text 1
assembly_name:
	.string "System.Runtime.Numerics"
.data 0
	.balign 8
mono_aot_file_info:
	.globl mono_aot_file_info
	.type mono_aot_file_info,@object

	.long 187,0
	.balign 8
	.xword mono_aot_System_Runtime_Numerics_got
	.balign 8
	.xword 0
	.balign 8
	.xword 0
	.balign 8
	.xword 0
	.balign 8
	.xword 0
	.balign 8
	.xword jit_code_start
	.balign 8
	.xword jit_code_end
	.balign 8
	.xword method_addresses
	.balign 8
	.xword 0
	.balign 8
	.xword 0
	.balign 8
	.xword blob
	.balign 8
	.xword class_name_table
	.balign 8
	.xword class_info_offsets
	.balign 8
	.xword method_info_offsets
	.balign 8
	.xword ex_info_offsets
	.balign 8
	.xword extra_method_info_offsets
	.balign 8
	.xword extra_method_table
	.balign 8
	.xword got_info_offsets
	.balign 8
	.xword 0
	.balign 8
	.xword image_table
	.balign 8
	.xword weak_field_indexes
	.balign 8
	.xword method_flags_table
	.balign 8
	.xword mem_end
	.balign 8
	.xword assembly_guid
	.balign 8
	.xword runtime_version
	.balign 8
	.xword 0
	.balign 8
	.xword 0
	.balign 8
	.xword 0
	.balign 8
	.xword 0
	.balign 8
	.xword 0
	.balign 8
	.xword 0
	.balign 8
	.xword 0
	.balign 8
	.xword assembly_name
	.balign 8
	.xword plt
	.balign 8
	.xword plt_end
	.balign 8
	.xword unwind_info
	.balign 8
	.xword unbox_trampolines
	.balign 8
	.xword unbox_trampolines_end
	.balign 8
	.xword unbox_trampoline_addresses

	.long 50,50,592,200,24,385,0,32
	.long 374417919,0,5522,128,8,8,7,9
	.long 8388607,0,4,25,0,0,0,0
	.long 0,0,0,0,0,0,0,0
	.long 0,0,0,0,0,0,0,0
	.long 0,0,0,0,0,0,0,0
	.long 0,0,0,0,0,0,0,0
	.long 0,0,0,0,0
	.byte 20,192,20,208,212,27,226,140,8,153,204,85,167,33,181,34
.section ".debug_info"
.subsection 0

	.byte 2
	.string "System.Collections.Generic.ValueListBuilder`1<T_REF>:.ctor"
	.string "System_Collections_Generic_ValueListBuilder_1_T_REF__ctor_System_Span_1_T_REF"

	.byte 0,0
	.string "System.Collections.Generic.ValueListBuilder`1<T_REF>:.ctor"
	.xword .Lm_78
	.xword .Lme_78

	.byte 2,118,16,3
	.string "this"

.LDIFF_SYM3=.LDIE_I - .Ldebug_info_start
	.long .LDIFF_SYM3
	.byte 1,106,3
	.string "param0"

.LDIFF_SYM4=.LDIE_I4 - .Ldebug_info_start
	.long .LDIFF_SYM4
	.byte 2,141,24,0

.section ".debug_frame"
.subsection 0

.LDIFF_SYM5=.Lfde0_end - .Lfde0_start
	.long .LDIFF_SYM5
.Lfde0_start:

	.long 0
	.balign 8
	.xword .Lm_78

.LDIFF_SYM6=.Lme_78 - .Lm_78
	.long .LDIFF_SYM6
	.long 0
	.byte 12,31,0,68,14,48,157,6,158,5,68,13,29,68,154,4
	.balign 8
.Lfde0_end:

.section ".debug_info"
.subsection 0

	.byte 2
	.string "System.Collections.Generic.ValueListBuilder`1<T_REF>:get_Length"
	.string "System_Collections_Generic_ValueListBuilder_1_T_REF_get_Length"

	.byte 0,0
	.string "System.Collections.Generic.ValueListBuilder`1<T_REF>:get_Length"
	.xword .Lm_79
	.xword .Lme_79

	.byte 2,118,16,3
	.string "this"

.LDIFF_SYM7=.LDIE_I - .Ldebug_info_start
	.long .LDIFF_SYM7
	.byte 2,141,16,0

.section ".debug_frame"
.subsection 0

.LDIFF_SYM8=.Lfde1_end - .Lfde1_start
	.long .LDIFF_SYM8
.Lfde1_start:

	.long 0
	.balign 8
	.xword .Lm_79

.LDIFF_SYM9=.Lme_79 - .Lm_79
	.long .LDIFF_SYM9
	.long 0
	.byte 12,31,0,68,14,32,157,4,158,3,68,13,29
	.balign 8
.Lfde1_end:

.section ".debug_info"
.subsection 0
.LTDIE_2:

	.byte 17
	.string "System_Object"

	.byte 16,7
	.string "System_Object"

.LDIFF_SYM10=.LTDIE_2 - .Ldebug_info_start
	.long .LDIFF_SYM10
.LTDIE_2_POINTER:

	.byte 13
.LDIFF_SYM11=.LTDIE_2 - .Ldebug_info_start
	.long .LDIFF_SYM11
.LTDIE_2_REFERENCE:

	.byte 14
.LDIFF_SYM12=.LTDIE_2 - .Ldebug_info_start
	.long .LDIFF_SYM12
.LTDIE_1:

	.byte 5
	.string "System_ValueType"

	.byte 16,16
.LDIFF_SYM13=.LTDIE_2 - .Ldebug_info_start
	.long .LDIFF_SYM13
	.byte 2,35,0,0,7
	.string "System_ValueType"

.LDIFF_SYM14=.LTDIE_1 - .Ldebug_info_start
	.long .LDIFF_SYM14
.LTDIE_1_POINTER:

	.byte 13
.LDIFF_SYM15=.LTDIE_1 - .Ldebug_info_start
	.long .LDIFF_SYM15
.LTDIE_1_REFERENCE:

	.byte 14
.LDIFF_SYM16=.LTDIE_1 - .Ldebug_info_start
	.long .LDIFF_SYM16
.LTDIE_0:

	.byte 5
	.string "System_Int32"

	.byte 20,16
.LDIFF_SYM17=.LTDIE_1 - .Ldebug_info_start
	.long .LDIFF_SYM17
	.byte 2,35,0,6
	.string "m_value"

.LDIFF_SYM18=.LDIE_I4 - .Ldebug_info_start
	.long .LDIFF_SYM18
	.byte 2,35,16,0,7
	.string "System_Int32"

.LDIFF_SYM19=.LTDIE_0 - .Ldebug_info_start
	.long .LDIFF_SYM19
.LTDIE_0_POINTER:

	.byte 13
.LDIFF_SYM20=.LTDIE_0 - .Ldebug_info_start
	.long .LDIFF_SYM20
.LTDIE_0_REFERENCE:

	.byte 14
.LDIFF_SYM21=.LTDIE_0 - .Ldebug_info_start
	.long .LDIFF_SYM21
	.byte 2
	.string "System.Collections.Generic.ValueListBuilder`1<T_REF>:Append"
	.string "System_Collections_Generic_ValueListBuilder_1_T_REF_Append_T_REF"

	.byte 0,0
	.string "System.Collections.Generic.ValueListBuilder`1<T_REF>:Append"
	.xword .Lm_7a
	.xword .Lme_7a

	.byte 2,118,16,3
	.string "this"

.LDIFF_SYM22=.LDIE_I - .Ldebug_info_start
	.long .LDIFF_SYM22
	.byte 1,105,3
	.string "param0"

.LDIFF_SYM23=.LDIE_I4 - .Ldebug_info_start
	.long .LDIFF_SYM23
	.byte 2,141,32,11
	.string "V_0"

.LDIFF_SYM24=.LDIE_I4 - .Ldebug_info_start
	.long .LDIFF_SYM24
	.byte 1,104,11
	.string "V_1"

.LDIFF_SYM25=.LDIE_I4 - .Ldebug_info_start
	.long .LDIFF_SYM25
	.byte 2,141,48,0

.section ".debug_frame"
.subsection 0

.LDIFF_SYM26=.Lfde2_end - .Lfde2_start
	.long .LDIFF_SYM26
.Lfde2_start:

	.long 0
	.balign 8
	.xword .Lm_7a

.LDIFF_SYM27=.Lme_7a - .Lm_7a
	.long .LDIFF_SYM27
	.long 0
	.byte 12,31,0,68,14,64,157,8,158,7,68,13,29,68,152,6,153,5
	.balign 8
.Lfde2_end:

.section ".debug_info"
.subsection 0

	.byte 2
	.string "System.Collections.Generic.ValueListBuilder`1<T_REF>:Append"
	.string "System_Collections_Generic_ValueListBuilder_1_T_REF_Append_System_ReadOnlySpan_1_T_REF"

	.byte 0,0
	.string "System.Collections.Generic.ValueListBuilder`1<T_REF>:Append"
	.xword .Lm_7b
	.xword .Lme_7b

	.byte 2,118,16,3
	.string "this"

.LDIFF_SYM28=.LDIE_I - .Ldebug_info_start
	.long .LDIFF_SYM28
	.byte 1,106,3
	.string "param0"

.LDIFF_SYM29=.LDIE_I4 - .Ldebug_info_start
	.long .LDIFF_SYM29
	.byte 2,141,32,11
	.string "V_0"

.LDIFF_SYM30=.LDIE_I4 - .Ldebug_info_start
	.long .LDIFF_SYM30
	.byte 1,105,11
	.string "V_1"

.LDIFF_SYM31=.LDIE_I4 - .Ldebug_info_start
	.long .LDIFF_SYM31
	.byte 3,141,200,0,0

.section ".debug_frame"
.subsection 0

.LDIFF_SYM32=.Lfde3_end - .Lfde3_start
	.long .LDIFF_SYM32
.Lfde3_start:

	.long 0
	.balign 8
	.xword .Lm_7b

.LDIFF_SYM33=.Lme_7b - .Lm_7b
	.long .LDIFF_SYM33
	.long 0
	.byte 12,31,0,68,14,112,157,14,158,13,68,13,29,68,153,12,154,11
	.balign 8
.Lfde3_end:

.section ".debug_info"
.subsection 0

	.byte 2
	.string "System.Collections.Generic.ValueListBuilder`1<T_REF>:AppendMultiChar"
	.string "System_Collections_Generic_ValueListBuilder_1_T_REF_AppendMultiChar_System_ReadOnlySpan_1_T_REF"

	.byte 0,0
	.string "System.Collections.Generic.ValueListBuilder`1<T_REF>:AppendMultiChar"
	.xword .Lm_7c
	.xword .Lme_7c

	.byte 2,118,16,3
	.string "this"

.LDIFF_SYM34=.LDIE_I - .Ldebug_info_start
	.long .LDIFF_SYM34
	.byte 1,106,3
	.string "param0"

.LDIFF_SYM35=.LDIE_I4 - .Ldebug_info_start
	.long .LDIFF_SYM35
	.byte 2,141,48,0

.section ".debug_frame"
.subsection 0

.LDIFF_SYM36=.Lfde4_end - .Lfde4_start
	.long .LDIFF_SYM36
.Lfde4_start:

	.long 0
	.balign 8
	.xword .Lm_7c

.LDIFF_SYM37=.Lme_7c - .Lm_7c
	.long .LDIFF_SYM37
	.long 0
	.byte 12,31,0,68,14,144,1,157,18,158,17,68,13,29,68,151,16,152,15,68,153,14,154,13
	.balign 8
.Lfde4_end:

.section ".debug_info"
.subsection 0

	.byte 2
	.string "System.Collections.Generic.ValueListBuilder`1<T_REF>:Insert"
	.string "System_Collections_Generic_ValueListBuilder_1_T_REF_Insert_int_System_ReadOnlySpan_1_T_REF"

	.byte 0,0
	.string "System.Collections.Generic.ValueListBuilder`1<T_REF>:Insert"
	.xword .Lm_7d
	.xword .Lme_7d

	.byte 2,118,16,3
	.string "this"

.LDIFF_SYM38=.LDIE_I - .Ldebug_info_start
	.long .LDIFF_SYM38
	.byte 1,106,3
	.string "param0"

.LDIFF_SYM39=.LDIE_I4 - .Ldebug_info_start
	.long .LDIFF_SYM39
	.byte 0,3
	.string "param1"

.LDIFF_SYM40=.LDIE_I4 - .Ldebug_info_start
	.long .LDIFF_SYM40
	.byte 2,141,56,11
	.string "V_0"

.LDIFF_SYM41=.LDIE_I4 - .Ldebug_info_start
	.long .LDIFF_SYM41
	.byte 3,141,192,1,0

.section ".debug_frame"
.subsection 0

.LDIFF_SYM42=.Lfde5_end - .Lfde5_start
	.long .LDIFF_SYM42
.Lfde5_start:

	.long 0
	.balign 8
	.xword .Lm_7d

.LDIFF_SYM43=.Lme_7d - .Lm_7d
	.long .LDIFF_SYM43
	.long 0
	.byte 12,31,0,68,14,224,1,157,28,158,27,68,13,29,68,151,26,152,25,68,153,24,154,23
	.balign 8
.Lfde5_end:

.section ".debug_info"
.subsection 0

	.byte 2
	.string "System.Collections.Generic.ValueListBuilder`1<T_REF>:AppendSpan"
	.string "System_Collections_Generic_ValueListBuilder_1_T_REF_AppendSpan_int"

	.byte 0,0
	.string "System.Collections.Generic.ValueListBuilder`1<T_REF>:AppendSpan"
	.xword .Lm_7e
	.xword .Lme_7e

	.byte 2,118,16,3
	.string "this"

.LDIFF_SYM44=.LDIE_I - .Ldebug_info_start
	.long .LDIFF_SYM44
	.byte 1,105,3
	.string "param0"

.LDIFF_SYM45=.LDIE_I4 - .Ldebug_info_start
	.long .LDIFF_SYM45
	.byte 1,106,11
	.string "V_0"

.LDIFF_SYM46=.LDIE_I4 - .Ldebug_info_start
	.long .LDIFF_SYM46
	.byte 1,104,11
	.string "V_1"

.LDIFF_SYM47=.LDIE_I4 - .Ldebug_info_start
	.long .LDIFF_SYM47
	.byte 3,141,216,0,0

.section ".debug_frame"
.subsection 0

.LDIFF_SYM48=.Lfde6_end - .Lfde6_start
	.long .LDIFF_SYM48
.Lfde6_start:

	.long 0
	.balign 8
	.xword .Lm_7e

.LDIFF_SYM49=.Lme_7e - .Lm_7e
	.long .LDIFF_SYM49
	.long 0
	.byte 12,31,0,68,14,112,157,14,158,13,68,13,29,68,151,12,152,11,68,153,10,154,9
	.balign 8
.Lfde6_end:

.section ".debug_info"
.subsection 0

	.byte 2
	.string "System.Collections.Generic.ValueListBuilder`1<T_REF>:AppendSpanWithGrow"
	.string "System_Collections_Generic_ValueListBuilder_1_T_REF_AppendSpanWithGrow_int"

	.byte 0,0
	.string "System.Collections.Generic.ValueListBuilder`1<T_REF>:AppendSpanWithGrow"
	.xword .Lm_7f
	.xword .Lme_7f

	.byte 2,118,16,3
	.string "this"

.LDIFF_SYM50=.LDIE_I - .Ldebug_info_start
	.long .LDIFF_SYM50
	.byte 1,105,3
	.string "param0"

.LDIFF_SYM51=.LDIE_I4 - .Ldebug_info_start
	.long .LDIFF_SYM51
	.byte 2,141,56,11
	.string "V_0"

.LDIFF_SYM52=.LDIE_I4 - .Ldebug_info_start
	.long .LDIFF_SYM52
	.byte 0,0

.section ".debug_frame"
.subsection 0

.LDIFF_SYM53=.Lfde7_end - .Lfde7_start
	.long .LDIFF_SYM53
.Lfde7_start:

	.long 0
	.balign 8
	.xword .Lm_7f

.LDIFF_SYM54=.Lme_7f - .Lm_7f
	.long .LDIFF_SYM54
	.long 0
	.byte 12,31,0,68,14,112,157,14,158,13,68,13,29,68,151,12,152,11,68,153,10
	.balign 8
.Lfde7_end:

.section ".debug_info"
.subsection 0

	.byte 2
	.string "System.Collections.Generic.ValueListBuilder`1<T_REF>:AddWithResize"
	.string "System_Collections_Generic_ValueListBuilder_1_T_REF_AddWithResize_T_REF"

	.byte 0,0
	.string "System.Collections.Generic.ValueListBuilder`1<T_REF>:AddWithResize"
	.xword .Lm_80
	.xword .Lme_80

	.byte 2,118,16,3
	.string "this"

.LDIFF_SYM55=.LDIE_I - .Ldebug_info_start
	.long .LDIFF_SYM55
	.byte 1,105,3
	.string "param0"

.LDIFF_SYM56=.LDIE_I4 - .Ldebug_info_start
	.long .LDIFF_SYM56
	.byte 2,141,24,11
	.string "V_0"

.LDIFF_SYM57=.LDIE_I4 - .Ldebug_info_start
	.long .LDIFF_SYM57
	.byte 0,0

.section ".debug_frame"
.subsection 0

.LDIFF_SYM58=.Lfde8_end - .Lfde8_start
	.long .LDIFF_SYM58
.Lfde8_start:

	.long 0
	.balign 8
	.xword .Lm_80

.LDIFF_SYM59=.Lme_80 - .Lm_80
	.long .LDIFF_SYM59
	.long 0
	.byte 12,31,0,68,14,64,157,8,158,7,68,13,29,68,153,6
	.balign 8
.Lfde8_end:

.section ".debug_info"
.subsection 0

	.byte 2
	.string "System.Collections.Generic.ValueListBuilder`1<T_REF>:AsSpan"
	.string "System_Collections_Generic_ValueListBuilder_1_T_REF_AsSpan"

	.byte 0,0
	.string "System.Collections.Generic.ValueListBuilder`1<T_REF>:AsSpan"
	.xword .Lm_81
	.xword .Lme_81

	.byte 2,118,16,3
	.string "this"

.LDIFF_SYM60=.LDIE_I - .Ldebug_info_start
	.long .LDIFF_SYM60
	.byte 1,106,0

.section ".debug_frame"
.subsection 0

.LDIFF_SYM61=.Lfde9_end - .Lfde9_start
	.long .LDIFF_SYM61
.Lfde9_start:

	.long 0
	.balign 8
	.xword .Lm_81

.LDIFF_SYM62=.Lme_81 - .Lm_81
	.long .LDIFF_SYM62
	.long 0
	.byte 12,31,0,68,14,112,157,14,158,13,68,13,29,68,152,12,153,11,68,154,10
	.balign 8
.Lfde9_end:

.section ".debug_info"
.subsection 0

	.byte 2
	.string "System.Collections.Generic.ValueListBuilder`1<T_REF>:TryCopyTo"
	.string "System_Collections_Generic_ValueListBuilder_1_T_REF_TryCopyTo_System_Span_1_T_REF_int_"

	.byte 0,0
	.string "System.Collections.Generic.ValueListBuilder`1<T_REF>:TryCopyTo"
	.xword .Lm_82
	.xword .Lme_82

	.byte 2,118,16,3
	.string "this"

.LDIFF_SYM63=.LDIE_I - .Ldebug_info_start
	.long .LDIFF_SYM63
	.byte 1,105,3
	.string "param0"

.LDIFF_SYM64=.LDIE_I4 - .Ldebug_info_start
	.long .LDIFF_SYM64
	.byte 2,141,48,3
	.string "param1"

.LDIFF_SYM65=.LDIE_I - .Ldebug_info_start
	.long .LDIFF_SYM65
	.byte 3,141,192,0,11
	.string "V_0"

.LDIFF_SYM66=.LDIE_I4 - .Ldebug_info_start
	.long .LDIFF_SYM66
	.byte 3,141,224,0,0

.section ".debug_frame"
.subsection 0

.LDIFF_SYM67=.Lfde10_end - .Lfde10_start
	.long .LDIFF_SYM67
.Lfde10_start:

	.long 0
	.balign 8
	.xword .Lm_82

.LDIFF_SYM68=.Lme_82 - .Lm_82
	.long .LDIFF_SYM68
	.long 0
	.byte 12,31,0,68,14,112,157,14,158,13,68,13,29,68,150,12,151,11,68,152,10,153,9
	.balign 8
.Lfde10_end:

.section ".debug_info"
.subsection 0

	.byte 2
	.string "System.Collections.Generic.ValueListBuilder`1<T_REF>:Dispose"
	.string "System_Collections_Generic_ValueListBuilder_1_T_REF_Dispose"

	.byte 0,0
	.string "System.Collections.Generic.ValueListBuilder`1<T_REF>:Dispose"
	.xword .Lm_83
	.xword .Lme_83

	.byte 2,118,16,3
	.string "this"

.LDIFF_SYM69=.LDIE_I - .Ldebug_info_start
	.long .LDIFF_SYM69
	.byte 2,141,24,11
	.string "V_0"

.LDIFF_SYM70=.LDIE_SZARRAY - .Ldebug_info_start
	.long .LDIFF_SYM70
	.byte 1,105,0

.section ".debug_frame"
.subsection 0

.LDIFF_SYM71=.Lfde11_end - .Lfde11_start
	.long .LDIFF_SYM71
.Lfde11_start:

	.long 0
	.balign 8
	.xword .Lm_83

.LDIFF_SYM72=.Lme_83 - .Lm_83
	.long .LDIFF_SYM72
	.long 0
	.byte 12,31,0,68,14,48,157,6,158,5,68,13,29,68,153,4
	.balign 8
.Lfde11_end:

.section ".debug_info"
.subsection 0

	.byte 2
	.string "System.Collections.Generic.ValueListBuilder`1<T_REF>:Grow"
	.string "System_Collections_Generic_ValueListBuilder_1_T_REF_Grow_int"

	.byte 0,0
	.string "System.Collections.Generic.ValueListBuilder`1<T_REF>:Grow"
	.xword .Lm_84
	.xword .Lme_84

	.byte 2,118,16,3
	.string "this"

.LDIFF_SYM73=.LDIE_I - .Ldebug_info_start
	.long .LDIFF_SYM73
	.byte 1,105,3
	.string "param0"

.LDIFF_SYM74=.LDIE_I4 - .Ldebug_info_start
	.long .LDIFF_SYM74
	.byte 1,106,11
	.string "V_0"

.LDIFF_SYM75=.LDIE_I4 - .Ldebug_info_start
	.long .LDIFF_SYM75
	.byte 1,106,11
	.string "V_1"

.LDIFF_SYM76=.LDIE_SZARRAY - .Ldebug_info_start
	.long .LDIFF_SYM76
	.byte 1,106,11
	.string "V_2"

.LDIFF_SYM77=.LDIE_SZARRAY - .Ldebug_info_start
	.long .LDIFF_SYM77
	.byte 1,104,11
	.string "V_3"

.LDIFF_SYM78=.LDIE_SZARRAY - .Ldebug_info_start
	.long .LDIFF_SYM78
	.byte 1,102,0

.section ".debug_frame"
.subsection 0

.LDIFF_SYM79=.Lfde12_end - .Lfde12_start
	.long .LDIFF_SYM79
.Lfde12_start:

	.long 0
	.balign 8
	.xword .Lm_84

.LDIFF_SYM80=.Lme_84 - .Lm_84
	.long .LDIFF_SYM80
	.long 0
	.byte 12,31,0,68,14,176,1,157,22,158,21,68,13,29,68,150,20,151,19,68,152,18,153,17,68,154,16
	.balign 8
.Lfde12_end:

.section ".debug_info"
.subsection 0

	.byte 2
	.string "System.Numerics.BigInteger:CreateChecked<TOther_REF>"
	.string "System_Numerics_BigInteger_CreateChecked_TOther_REF_TOther_REF"

	.byte 0,0
	.string "System.Numerics.BigInteger:CreateChecked<TOther_REF>"
	.xword .Lm_129
	.xword .Lme_129

	.byte 2,118,16,3
	.string "param0"

.LDIFF_SYM81=.LDIE_I4 - .Ldebug_info_start
	.long .LDIFF_SYM81
	.byte 2,141,32,11
	.string "V_0"

.LDIFF_SYM82=.LDIE_I4 - .Ldebug_info_start
	.long .LDIFF_SYM82
	.byte 2,141,48,0

.section ".debug_frame"
.subsection 0

.LDIFF_SYM83=.Lfde13_end - .Lfde13_start
	.long .LDIFF_SYM83
.Lfde13_start:

	.long 0
	.balign 8
	.xword .Lm_129

.LDIFF_SYM84=.Lme_129 - .Lm_129
	.long .LDIFF_SYM84
	.long 0
	.byte 12,31,0,68,14,176,2,157,38,158,37,68,13,29
	.balign 8
.Lfde13_end:

.section ".debug_info"
.subsection 0

	.byte 2
	.string "System.Numerics.BigInteger:CreateSaturating<TOther_REF>"
	.string "System_Numerics_BigInteger_CreateSaturating_TOther_REF_TOther_REF"

	.byte 0,0
	.string "System.Numerics.BigInteger:CreateSaturating<TOther_REF>"
	.xword .Lm_12a
	.xword .Lme_12a

	.byte 2,118,16,3
	.string "param0"

.LDIFF_SYM85=.LDIE_I4 - .Ldebug_info_start
	.long .LDIFF_SYM85
	.byte 2,141,32,11
	.string "V_0"

.LDIFF_SYM86=.LDIE_I4 - .Ldebug_info_start
	.long .LDIFF_SYM86
	.byte 2,141,48,0

.section ".debug_frame"
.subsection 0

.LDIFF_SYM87=.Lfde14_end - .Lfde14_start
	.long .LDIFF_SYM87
.Lfde14_start:

	.long 0
	.balign 8
	.xword .Lm_12a

.LDIFF_SYM88=.Lme_12a - .Lm_12a
	.long .LDIFF_SYM88
	.long 0
	.byte 12,31,0,68,14,176,2,157,38,158,37,68,13,29
	.balign 8
.Lfde14_end:

.section ".debug_info"
.subsection 0

	.byte 2
	.string "System.Numerics.BigInteger:CreateTruncating<TOther_REF>"
	.string "System_Numerics_BigInteger_CreateTruncating_TOther_REF_TOther_REF"

	.byte 0,0
	.string "System.Numerics.BigInteger:CreateTruncating<TOther_REF>"
	.xword .Lm_12b
	.xword .Lme_12b

	.byte 2,118,16,3
	.string "param0"

.LDIFF_SYM89=.LDIE_I4 - .Ldebug_info_start
	.long .LDIFF_SYM89
	.byte 2,141,32,11
	.string "V_0"

.LDIFF_SYM90=.LDIE_I4 - .Ldebug_info_start
	.long .LDIFF_SYM90
	.byte 2,141,48,0

.section ".debug_frame"
.subsection 0

.LDIFF_SYM91=.Lfde15_end - .Lfde15_start
	.long .LDIFF_SYM91
.Lfde15_start:

	.long 0
	.balign 8
	.xword .Lm_12b

.LDIFF_SYM92=.Lme_12b - .Lm_12b
	.long .LDIFF_SYM92
	.long 0
	.byte 12,31,0,68,14,176,2,157,38,158,37,68,13,29
	.balign 8
.Lfde15_end:

.section ".debug_info"
.subsection 0
.LTDIE_3:

	.byte 5
	.string "System_Numerics_BigInteger"

	.byte 32,16
.LDIFF_SYM93=.LTDIE_1 - .Ldebug_info_start
	.long .LDIFF_SYM93
	.byte 2,35,0,6
	.string "_sign"

.LDIFF_SYM94=.LDIE_I4 - .Ldebug_info_start
	.long .LDIFF_SYM94
	.byte 2,35,0,6
	.string "_bits"

.LDIFF_SYM95=.LDIE_SZARRAY - .Ldebug_info_start
	.long .LDIFF_SYM95
	.byte 2,35,8,0,7
	.string "System_Numerics_BigInteger"

.LDIFF_SYM96=.LTDIE_3 - .Ldebug_info_start
	.long .LDIFF_SYM96
.LTDIE_3_POINTER:

	.byte 13
.LDIFF_SYM97=.LTDIE_3 - .Ldebug_info_start
	.long .LDIFF_SYM97
.LTDIE_3_REFERENCE:

	.byte 14
.LDIFF_SYM98=.LTDIE_3 - .Ldebug_info_start
	.long .LDIFF_SYM98
	.byte 2
	.string "System.Numerics.BigInteger:System.Numerics.INumberBase<System.Numerics.BigInteger>.TryConvertFromChecked<TOther_REF>"
	.string "System_Numerics_BigInteger_System_Numerics_INumberBase_System_Numerics_BigInteger_TryConvertFromChecked_TOther_REF_TOther_REF_System_Numerics_BigInteger_"

	.byte 0,0
	.string "System.Numerics.BigInteger:System.Numerics.INumberBase<System.Numerics.BigInteger>.TryConvertFromChecked<TOther_REF>"
	.xword .Lm_130
	.xword .Lme_130

	.byte 2,118,16,3
	.string "param0"

.LDIFF_SYM99=.LDIE_I4 - .Ldebug_info_start
	.long .LDIFF_SYM99
	.byte 0,3
	.string "param1"

.LDIFF_SYM100=.LDIE_I - .Ldebug_info_start
	.long .LDIFF_SYM100
	.byte 2,141,24,0

.section ".debug_frame"
.subsection 0

.LDIFF_SYM101=.Lfde16_end - .Lfde16_start
	.long .LDIFF_SYM101
.Lfde16_start:

	.long 0
	.balign 8
	.xword .Lm_130

.LDIFF_SYM102=.Lme_130 - .Lm_130
	.long .LDIFF_SYM102
	.long 0
	.byte 12,31,0,68,14,144,2,157,34,158,33,68,13,29
	.balign 8
.Lfde16_end:

.section ".debug_info"
.subsection 0
.LTDIE_4:

	.byte 5
	.string "System_Byte"

	.byte 17,16
.LDIFF_SYM103=.LTDIE_1 - .Ldebug_info_start
	.long .LDIFF_SYM103
	.byte 2,35,0,6
	.string "m_value"

.LDIFF_SYM104=.LDIE_U1 - .Ldebug_info_start
	.long .LDIFF_SYM104
	.byte 2,35,16,0,7
	.string "System_Byte"

.LDIFF_SYM105=.LTDIE_4 - .Ldebug_info_start
	.long .LDIFF_SYM105
.LTDIE_4_POINTER:

	.byte 13
.LDIFF_SYM106=.LTDIE_4 - .Ldebug_info_start
	.long .LDIFF_SYM106
.LTDIE_4_REFERENCE:

	.byte 14
.LDIFF_SYM107=.LTDIE_4 - .Ldebug_info_start
	.long .LDIFF_SYM107
.LTDIE_5:

	.byte 5
	.string "System_Char"

	.byte 18,16
.LDIFF_SYM108=.LTDIE_1 - .Ldebug_info_start
	.long .LDIFF_SYM108
	.byte 2,35,0,6
	.string "m_value"

.LDIFF_SYM109=.LDIE_CHAR - .Ldebug_info_start
	.long .LDIFF_SYM109
	.byte 2,35,16,0,7
	.string "System_Char"

.LDIFF_SYM110=.LTDIE_5 - .Ldebug_info_start
	.long .LDIFF_SYM110
.LTDIE_5_POINTER:

	.byte 13
.LDIFF_SYM111=.LTDIE_5 - .Ldebug_info_start
	.long .LDIFF_SYM111
.LTDIE_5_REFERENCE:

	.byte 14
.LDIFF_SYM112=.LTDIE_5 - .Ldebug_info_start
	.long .LDIFF_SYM112
.LTDIE_6:

	.byte 5
	.string "System_Double"

	.byte 24,16
.LDIFF_SYM113=.LTDIE_1 - .Ldebug_info_start
	.long .LDIFF_SYM113
	.byte 2,35,0,6
	.string "m_value"

.LDIFF_SYM114=.LDIE_R8 - .Ldebug_info_start
	.long .LDIFF_SYM114
	.byte 2,35,16,0,7
	.string "System_Double"

.LDIFF_SYM115=.LTDIE_6 - .Ldebug_info_start
	.long .LDIFF_SYM115
.LTDIE_6_POINTER:

	.byte 13
.LDIFF_SYM116=.LTDIE_6 - .Ldebug_info_start
	.long .LDIFF_SYM116
.LTDIE_6_REFERENCE:

	.byte 14
.LDIFF_SYM117=.LTDIE_6 - .Ldebug_info_start
	.long .LDIFF_SYM117
.LTDIE_7:

	.byte 5
	.string "System_Int16"

	.byte 18,16
.LDIFF_SYM118=.LTDIE_1 - .Ldebug_info_start
	.long .LDIFF_SYM118
	.byte 2,35,0,6
	.string "m_value"

.LDIFF_SYM119=.LDIE_I2 - .Ldebug_info_start
	.long .LDIFF_SYM119
	.byte 2,35,16,0,7
	.string "System_Int16"

.LDIFF_SYM120=.LTDIE_7 - .Ldebug_info_start
	.long .LDIFF_SYM120
.LTDIE_7_POINTER:

	.byte 13
.LDIFF_SYM121=.LTDIE_7 - .Ldebug_info_start
	.long .LDIFF_SYM121
.LTDIE_7_REFERENCE:

	.byte 14
.LDIFF_SYM122=.LTDIE_7 - .Ldebug_info_start
	.long .LDIFF_SYM122
.LTDIE_8:

	.byte 5
	.string "System_Int64"

	.byte 24,16
.LDIFF_SYM123=.LTDIE_1 - .Ldebug_info_start
	.long .LDIFF_SYM123
	.byte 2,35,0,6
	.string "m_value"

.LDIFF_SYM124=.LDIE_I8 - .Ldebug_info_start
	.long .LDIFF_SYM124
	.byte 2,35,16,0,7
	.string "System_Int64"

.LDIFF_SYM125=.LTDIE_8 - .Ldebug_info_start
	.long .LDIFF_SYM125
.LTDIE_8_POINTER:

	.byte 13
.LDIFF_SYM126=.LTDIE_8 - .Ldebug_info_start
	.long .LDIFF_SYM126
.LTDIE_8_REFERENCE:

	.byte 14
.LDIFF_SYM127=.LTDIE_8 - .Ldebug_info_start
	.long .LDIFF_SYM127
.LTDIE_9:

	.byte 5
	.string "System_SByte"

	.byte 17,16
.LDIFF_SYM128=.LTDIE_1 - .Ldebug_info_start
	.long .LDIFF_SYM128
	.byte 2,35,0,6
	.string "m_value"

.LDIFF_SYM129=.LDIE_I1 - .Ldebug_info_start
	.long .LDIFF_SYM129
	.byte 2,35,16,0,7
	.string "System_SByte"

.LDIFF_SYM130=.LTDIE_9 - .Ldebug_info_start
	.long .LDIFF_SYM130
.LTDIE_9_POINTER:

	.byte 13
.LDIFF_SYM131=.LTDIE_9 - .Ldebug_info_start
	.long .LDIFF_SYM131
.LTDIE_9_REFERENCE:

	.byte 14
.LDIFF_SYM132=.LTDIE_9 - .Ldebug_info_start
	.long .LDIFF_SYM132
.LTDIE_10:

	.byte 5
	.string "System_Single"

	.byte 20,16
.LDIFF_SYM133=.LTDIE_1 - .Ldebug_info_start
	.long .LDIFF_SYM133
	.byte 2,35,0,6
	.string "m_value"

.LDIFF_SYM134=.LDIE_R4 - .Ldebug_info_start
	.long .LDIFF_SYM134
	.byte 2,35,16,0,7
	.string "System_Single"

.LDIFF_SYM135=.LTDIE_10 - .Ldebug_info_start
	.long .LDIFF_SYM135
.LTDIE_10_POINTER:

	.byte 13
.LDIFF_SYM136=.LTDIE_10 - .Ldebug_info_start
	.long .LDIFF_SYM136
.LTDIE_10_REFERENCE:

	.byte 14
.LDIFF_SYM137=.LTDIE_10 - .Ldebug_info_start
	.long .LDIFF_SYM137
.LTDIE_11:

	.byte 5
	.string "System_UInt16"

	.byte 18,16
.LDIFF_SYM138=.LTDIE_1 - .Ldebug_info_start
	.long .LDIFF_SYM138
	.byte 2,35,0,6
	.string "m_value"

.LDIFF_SYM139=.LDIE_U2 - .Ldebug_info_start
	.long .LDIFF_SYM139
	.byte 2,35,16,0,7
	.string "System_UInt16"

.LDIFF_SYM140=.LTDIE_11 - .Ldebug_info_start
	.long .LDIFF_SYM140
.LTDIE_11_POINTER:

	.byte 13
.LDIFF_SYM141=.LTDIE_11 - .Ldebug_info_start
	.long .LDIFF_SYM141
.LTDIE_11_REFERENCE:

	.byte 14
.LDIFF_SYM142=.LTDIE_11 - .Ldebug_info_start
	.long .LDIFF_SYM142
.LTDIE_12:

	.byte 5
	.string "System_UInt32"

	.byte 20,16
.LDIFF_SYM143=.LTDIE_1 - .Ldebug_info_start
	.long .LDIFF_SYM143
	.byte 2,35,0,6
	.string "m_value"

.LDIFF_SYM144=.LDIE_U4 - .Ldebug_info_start
	.long .LDIFF_SYM144
	.byte 2,35,16,0,7
	.string "System_UInt32"

.LDIFF_SYM145=.LTDIE_12 - .Ldebug_info_start
	.long .LDIFF_SYM145
.LTDIE_12_POINTER:

	.byte 13
.LDIFF_SYM146=.LTDIE_12 - .Ldebug_info_start
	.long .LDIFF_SYM146
.LTDIE_12_REFERENCE:

	.byte 14
.LDIFF_SYM147=.LTDIE_12 - .Ldebug_info_start
	.long .LDIFF_SYM147
.LTDIE_13:

	.byte 5
	.string "System_UInt64"

	.byte 24,16
.LDIFF_SYM148=.LTDIE_1 - .Ldebug_info_start
	.long .LDIFF_SYM148
	.byte 2,35,0,6
	.string "m_value"

.LDIFF_SYM149=.LDIE_U8 - .Ldebug_info_start
	.long .LDIFF_SYM149
	.byte 2,35,16,0,7
	.string "System_UInt64"

.LDIFF_SYM150=.LTDIE_13 - .Ldebug_info_start
	.long .LDIFF_SYM150
.LTDIE_13_POINTER:

	.byte 13
.LDIFF_SYM151=.LTDIE_13 - .Ldebug_info_start
	.long .LDIFF_SYM151
.LTDIE_13_REFERENCE:

	.byte 14
.LDIFF_SYM152=.LTDIE_13 - .Ldebug_info_start
	.long .LDIFF_SYM152
	.byte 2
	.string "System.Numerics.BigInteger:TryConvertFromChecked<TOther_REF>"
	.string "System_Numerics_BigInteger_TryConvertFromChecked_TOther_REF_TOther_REF_System_Numerics_BigInteger_"

	.byte 0,0
	.string "System.Numerics.BigInteger:TryConvertFromChecked<TOther_REF>"
	.xword .Lm_131
	.xword .Lme_131

	.byte 2,118,16,3
	.string "param0"

.LDIFF_SYM153=.LDIE_I4 - .Ldebug_info_start
	.long .LDIFF_SYM153
	.byte 0,3
	.string "param1"

.LDIFF_SYM154=.LDIE_I - .Ldebug_info_start
	.long .LDIFF_SYM154
	.byte 2,141,24,11
	.string "V_0"

.LDIFF_SYM155=.LDIE_I4 - .Ldebug_info_start
	.long .LDIFF_SYM155
	.byte 0,11
	.string "V_1"

.LDIFF_SYM156=.LDIE_I4 - .Ldebug_info_start
	.long .LDIFF_SYM156
	.byte 0,11
	.string "V_2"

.LDIFF_SYM157=.LDIE_I4 - .Ldebug_info_start
	.long .LDIFF_SYM157
	.byte 0,11
	.string "V_3"

.LDIFF_SYM158=.LDIE_I4 - .Ldebug_info_start
	.long .LDIFF_SYM158
	.byte 0,11
	.string "V_4"

.LDIFF_SYM159=.LDIE_I4 - .Ldebug_info_start
	.long .LDIFF_SYM159
	.byte 0,11
	.string "V_5"

.LDIFF_SYM160=.LDIE_I4 - .Ldebug_info_start
	.long .LDIFF_SYM160
	.byte 0,11
	.string "V_6"

.LDIFF_SYM161=.LDIE_I4 - .Ldebug_info_start
	.long .LDIFF_SYM161
	.byte 0,11
	.string "V_7"

.LDIFF_SYM162=.LDIE_I4 - .Ldebug_info_start
	.long .LDIFF_SYM162
	.byte 0,11
	.string "V_8"

.LDIFF_SYM163=.LDIE_I4 - .Ldebug_info_start
	.long .LDIFF_SYM163
	.byte 0,11
	.string "V_9"

.LDIFF_SYM164=.LDIE_I4 - .Ldebug_info_start
	.long .LDIFF_SYM164
	.byte 0,11
	.string "V_10"

.LDIFF_SYM165=.LDIE_I4 - .Ldebug_info_start
	.long .LDIFF_SYM165
	.byte 0,11
	.string "V_11"

.LDIFF_SYM166=.LDIE_I4 - .Ldebug_info_start
	.long .LDIFF_SYM166
	.byte 0,11
	.string "V_12"

.LDIFF_SYM167=.LDIE_I4 - .Ldebug_info_start
	.long .LDIFF_SYM167
	.byte 0,11
	.string "V_13"

.LDIFF_SYM168=.LDIE_I4 - .Ldebug_info_start
	.long .LDIFF_SYM168
	.byte 0,11
	.string "V_14"

.LDIFF_SYM169=.LDIE_I4 - .Ldebug_info_start
	.long .LDIFF_SYM169
	.byte 0,11
	.string "V_15"

.LDIFF_SYM170=.LDIE_I4 - .Ldebug_info_start
	.long .LDIFF_SYM170
	.byte 0,11
	.string "V_16"

.LDIFF_SYM171=.LDIE_I4 - .Ldebug_info_start
	.long .LDIFF_SYM171
	.byte 0,0

.section ".debug_frame"
.subsection 0

.LDIFF_SYM172=.Lfde17_end - .Lfde17_start
	.long .LDIFF_SYM172
.Lfde17_start:

	.long 0
	.balign 8
	.xword .Lm_131

.LDIFF_SYM173=.Lme_131 - .Lm_131
	.long .LDIFF_SYM173
	.long 0
	.byte 12,31,0,68,14,144,2,157,34,158,33,68,13,29
	.balign 8
.Lfde17_end:

.section ".debug_info"
.subsection 0

	.byte 2
	.string "System.Numerics.BigInteger:System.Numerics.INumberBase<System.Numerics.BigInteger>.TryConvertFromSaturating<TOther_REF>"
	.string "System_Numerics_BigInteger_System_Numerics_INumberBase_System_Numerics_BigInteger_TryConvertFromSaturating_TOther_REF_TOther_REF_System_Numerics_BigInteger_"

	.byte 0,0
	.string "System.Numerics.BigInteger:System.Numerics.INumberBase<System.Numerics.BigInteger>.TryConvertFromSaturating<TOther_REF>"
	.xword .Lm_132
	.xword .Lme_132

	.byte 2,118,16,3
	.string "param0"

.LDIFF_SYM174=.LDIE_I4 - .Ldebug_info_start
	.long .LDIFF_SYM174
	.byte 0,3
	.string "param1"

.LDIFF_SYM175=.LDIE_I - .Ldebug_info_start
	.long .LDIFF_SYM175
	.byte 2,141,24,0

.section ".debug_frame"
.subsection 0

.LDIFF_SYM176=.Lfde18_end - .Lfde18_start
	.long .LDIFF_SYM176
.Lfde18_start:

	.long 0
	.balign 8
	.xword .Lm_132

.LDIFF_SYM177=.Lme_132 - .Lm_132
	.long .LDIFF_SYM177
	.long 0
	.byte 12,31,0,68,14,160,2,157,36,158,35,68,13,29
	.balign 8
.Lfde18_end:

.section ".debug_info"
.subsection 0

	.byte 2
	.string "System.Numerics.BigInteger:TryConvertFromSaturating<TOther_REF>"
	.string "System_Numerics_BigInteger_TryConvertFromSaturating_TOther_REF_TOther_REF_System_Numerics_BigInteger_"

	.byte 0,0
	.string "System.Numerics.BigInteger:TryConvertFromSaturating<TOther_REF>"
	.xword .Lm_133
	.xword .Lme_133

	.byte 2,118,16,3
	.string "param0"

.LDIFF_SYM178=.LDIE_I4 - .Ldebug_info_start
	.long .LDIFF_SYM178
	.byte 0,3
	.string "param1"

.LDIFF_SYM179=.LDIE_I - .Ldebug_info_start
	.long .LDIFF_SYM179
	.byte 2,141,24,11
	.string "V_0"

.LDIFF_SYM180=.LDIE_I4 - .Ldebug_info_start
	.long .LDIFF_SYM180
	.byte 0,11
	.string "V_1"

.LDIFF_SYM181=.LDIE_I4 - .Ldebug_info_start
	.long .LDIFF_SYM181
	.byte 0,11
	.string "V_2"

.LDIFF_SYM182=.LDIE_I4 - .Ldebug_info_start
	.long .LDIFF_SYM182
	.byte 0,11
	.string "V_3"

.LDIFF_SYM183=.LDIE_I4 - .Ldebug_info_start
	.long .LDIFF_SYM183
	.byte 0,11
	.string "V_4"

.LDIFF_SYM184=.LDIE_I4 - .Ldebug_info_start
	.long .LDIFF_SYM184
	.byte 0,11
	.string "V_5"

.LDIFF_SYM185=.LDIE_I4 - .Ldebug_info_start
	.long .LDIFF_SYM185
	.byte 0,11
	.string "V_6"

.LDIFF_SYM186=.LDIE_I4 - .Ldebug_info_start
	.long .LDIFF_SYM186
	.byte 0,11
	.string "V_7"

.LDIFF_SYM187=.LDIE_I4 - .Ldebug_info_start
	.long .LDIFF_SYM187
	.byte 0,11
	.string "V_8"

.LDIFF_SYM188=.LDIE_I4 - .Ldebug_info_start
	.long .LDIFF_SYM188
	.byte 0,11
	.string "V_9"

.LDIFF_SYM189=.LDIE_I4 - .Ldebug_info_start
	.long .LDIFF_SYM189
	.byte 0,11
	.string "V_10"

.LDIFF_SYM190=.LDIE_I4 - .Ldebug_info_start
	.long .LDIFF_SYM190
	.byte 0,11
	.string "V_11"

.LDIFF_SYM191=.LDIE_I4 - .Ldebug_info_start
	.long .LDIFF_SYM191
	.byte 0,11
	.string "V_12"

.LDIFF_SYM192=.LDIE_I4 - .Ldebug_info_start
	.long .LDIFF_SYM192
	.byte 0,11
	.string "V_13"

.LDIFF_SYM193=.LDIE_I4 - .Ldebug_info_start
	.long .LDIFF_SYM193
	.byte 0,11
	.string "V_14"

.LDIFF_SYM194=.LDIE_I4 - .Ldebug_info_start
	.long .LDIFF_SYM194
	.byte 0,11
	.string "V_15"

.LDIFF_SYM195=.LDIE_I4 - .Ldebug_info_start
	.long .LDIFF_SYM195
	.byte 0,11
	.string "V_16"

.LDIFF_SYM196=.LDIE_I4 - .Ldebug_info_start
	.long .LDIFF_SYM196
	.byte 0,0

.section ".debug_frame"
.subsection 0

.LDIFF_SYM197=.Lfde19_end - .Lfde19_start
	.long .LDIFF_SYM197
.Lfde19_start:

	.long 0
	.balign 8
	.xword .Lm_133

.LDIFF_SYM198=.Lme_133 - .Lm_133
	.long .LDIFF_SYM198
	.long 0
	.byte 12,31,0,68,14,160,2,157,36,158,35,68,13,29
	.balign 8
.Lfde19_end:

.section ".debug_info"
.subsection 0

	.byte 2
	.string "System.Numerics.BigInteger:System.Numerics.INumberBase<System.Numerics.BigInteger>.TryConvertFromTruncating<TOther_REF>"
	.string "System_Numerics_BigInteger_System_Numerics_INumberBase_System_Numerics_BigInteger_TryConvertFromTruncating_TOther_REF_TOther_REF_System_Numerics_BigInteger_"

	.byte 0,0
	.string "System.Numerics.BigInteger:System.Numerics.INumberBase<System.Numerics.BigInteger>.TryConvertFromTruncating<TOther_REF>"
	.xword .Lm_134
	.xword .Lme_134

	.byte 2,118,16,3
	.string "param0"

.LDIFF_SYM199=.LDIE_I4 - .Ldebug_info_start
	.long .LDIFF_SYM199
	.byte 0,3
	.string "param1"

.LDIFF_SYM200=.LDIE_I - .Ldebug_info_start
	.long .LDIFF_SYM200
	.byte 2,141,24,0

.section ".debug_frame"
.subsection 0

.LDIFF_SYM201=.Lfde20_end - .Lfde20_start
	.long .LDIFF_SYM201
.Lfde20_start:

	.long 0
	.balign 8
	.xword .Lm_134

.LDIFF_SYM202=.Lme_134 - .Lm_134
	.long .LDIFF_SYM202
	.long 0
	.byte 12,31,0,68,14,160,2,157,36,158,35,68,13,29
	.balign 8
.Lfde20_end:

.section ".debug_info"
.subsection 0

	.byte 2
	.string "System.Numerics.BigInteger:TryConvertFromTruncating<TOther_REF>"
	.string "System_Numerics_BigInteger_TryConvertFromTruncating_TOther_REF_TOther_REF_System_Numerics_BigInteger_"

	.byte 0,0
	.string "System.Numerics.BigInteger:TryConvertFromTruncating<TOther_REF>"
	.xword .Lm_135
	.xword .Lme_135

	.byte 2,118,16,3
	.string "param0"

.LDIFF_SYM203=.LDIE_I4 - .Ldebug_info_start
	.long .LDIFF_SYM203
	.byte 0,3
	.string "param1"

.LDIFF_SYM204=.LDIE_I - .Ldebug_info_start
	.long .LDIFF_SYM204
	.byte 2,141,24,11
	.string "V_0"

.LDIFF_SYM205=.LDIE_I4 - .Ldebug_info_start
	.long .LDIFF_SYM205
	.byte 0,11
	.string "V_1"

.LDIFF_SYM206=.LDIE_I4 - .Ldebug_info_start
	.long .LDIFF_SYM206
	.byte 0,11
	.string "V_2"

.LDIFF_SYM207=.LDIE_I4 - .Ldebug_info_start
	.long .LDIFF_SYM207
	.byte 0,11
	.string "V_3"

.LDIFF_SYM208=.LDIE_I4 - .Ldebug_info_start
	.long .LDIFF_SYM208
	.byte 0,11
	.string "V_4"

.LDIFF_SYM209=.LDIE_I4 - .Ldebug_info_start
	.long .LDIFF_SYM209
	.byte 0,11
	.string "V_5"

.LDIFF_SYM210=.LDIE_I4 - .Ldebug_info_start
	.long .LDIFF_SYM210
	.byte 0,11
	.string "V_6"

.LDIFF_SYM211=.LDIE_I4 - .Ldebug_info_start
	.long .LDIFF_SYM211
	.byte 0,11
	.string "V_7"

.LDIFF_SYM212=.LDIE_I4 - .Ldebug_info_start
	.long .LDIFF_SYM212
	.byte 0,11
	.string "V_8"

.LDIFF_SYM213=.LDIE_I4 - .Ldebug_info_start
	.long .LDIFF_SYM213
	.byte 0,11
	.string "V_9"

.LDIFF_SYM214=.LDIE_I4 - .Ldebug_info_start
	.long .LDIFF_SYM214
	.byte 0,11
	.string "V_10"

.LDIFF_SYM215=.LDIE_I4 - .Ldebug_info_start
	.long .LDIFF_SYM215
	.byte 0,11
	.string "V_11"

.LDIFF_SYM216=.LDIE_I4 - .Ldebug_info_start
	.long .LDIFF_SYM216
	.byte 0,11
	.string "V_12"

.LDIFF_SYM217=.LDIE_I4 - .Ldebug_info_start
	.long .LDIFF_SYM217
	.byte 0,11
	.string "V_13"

.LDIFF_SYM218=.LDIE_I4 - .Ldebug_info_start
	.long .LDIFF_SYM218
	.byte 0,11
	.string "V_14"

.LDIFF_SYM219=.LDIE_I4 - .Ldebug_info_start
	.long .LDIFF_SYM219
	.byte 0,11
	.string "V_15"

.LDIFF_SYM220=.LDIE_I4 - .Ldebug_info_start
	.long .LDIFF_SYM220
	.byte 0,11
	.string "V_16"

.LDIFF_SYM221=.LDIE_I4 - .Ldebug_info_start
	.long .LDIFF_SYM221
	.byte 0,0

.section ".debug_frame"
.subsection 0

.LDIFF_SYM222=.Lfde21_end - .Lfde21_start
	.long .LDIFF_SYM222
.Lfde21_start:

	.long 0
	.balign 8
	.xword .Lm_135

.LDIFF_SYM223=.Lme_135 - .Lm_135
	.long .LDIFF_SYM223
	.long 0
	.byte 12,31,0,68,14,160,2,157,36,158,35,68,13,29
	.balign 8
.Lfde21_end:

.section ".debug_info"
.subsection 0

	.byte 2
	.string "System.Numerics.BigInteger:System.Numerics.INumberBase<System.Numerics.BigInteger>.TryConvertToChecked<TOther_REF>"
	.string "System_Numerics_BigInteger_System_Numerics_INumberBase_System_Numerics_BigInteger_TryConvertToChecked_TOther_REF_System_Numerics_BigInteger_TOther_REF_"

	.byte 0,0
	.string "System.Numerics.BigInteger:System.Numerics.INumberBase<System.Numerics.BigInteger>.TryConvertToChecked<TOther_REF>"
	.xword .Lm_136
	.xword .Lme_136

	.byte 2,118,16,3
	.string "param0"

.LDIFF_SYM224=.LDIE_I4 - .Ldebug_info_start
	.long .LDIFF_SYM224
	.byte 0,3
	.string "param1"

.LDIFF_SYM225=.LDIE_I - .Ldebug_info_start
	.long .LDIFF_SYM225
	.byte 2,141,32,11
	.string "V_0"

.LDIFF_SYM226=.LDIE_I4 - .Ldebug_info_start
	.long .LDIFF_SYM226
	.byte 0,11
	.string "V_1"

.LDIFF_SYM227=.LDIE_I4 - .Ldebug_info_start
	.long .LDIFF_SYM227
	.byte 0,11
	.string "V_2"

.LDIFF_SYM228=.LDIE_I4 - .Ldebug_info_start
	.long .LDIFF_SYM228
	.byte 0,11
	.string "V_3"

.LDIFF_SYM229=.LDIE_I4 - .Ldebug_info_start
	.long .LDIFF_SYM229
	.byte 0,11
	.string "V_4"

.LDIFF_SYM230=.LDIE_I4 - .Ldebug_info_start
	.long .LDIFF_SYM230
	.byte 0,11
	.string "V_5"

.LDIFF_SYM231=.LDIE_I4 - .Ldebug_info_start
	.long .LDIFF_SYM231
	.byte 0,11
	.string "V_6"

.LDIFF_SYM232=.LDIE_I4 - .Ldebug_info_start
	.long .LDIFF_SYM232
	.byte 0,11
	.string "V_7"

.LDIFF_SYM233=.LDIE_I4 - .Ldebug_info_start
	.long .LDIFF_SYM233
	.byte 0,11
	.string "V_8"

.LDIFF_SYM234=.LDIE_I4 - .Ldebug_info_start
	.long .LDIFF_SYM234
	.byte 0,11
	.string "V_9"

.LDIFF_SYM235=.LDIE_I4 - .Ldebug_info_start
	.long .LDIFF_SYM235
	.byte 0,11
	.string "V_10"

.LDIFF_SYM236=.LDIE_I4 - .Ldebug_info_start
	.long .LDIFF_SYM236
	.byte 0,11
	.string "V_11"

.LDIFF_SYM237=.LDIE_I4 - .Ldebug_info_start
	.long .LDIFF_SYM237
	.byte 0,11
	.string "V_12"

.LDIFF_SYM238=.LDIE_I4 - .Ldebug_info_start
	.long .LDIFF_SYM238
	.byte 0,11
	.string "V_13"

.LDIFF_SYM239=.LDIE_I4 - .Ldebug_info_start
	.long .LDIFF_SYM239
	.byte 0,11
	.string "V_14"

.LDIFF_SYM240=.LDIE_I4 - .Ldebug_info_start
	.long .LDIFF_SYM240
	.byte 0,11
	.string "V_15"

.LDIFF_SYM241=.LDIE_I4 - .Ldebug_info_start
	.long .LDIFF_SYM241
	.byte 0,11
	.string "V_16"

.LDIFF_SYM242=.LDIE_I4 - .Ldebug_info_start
	.long .LDIFF_SYM242
	.byte 0,11
	.string "V_17"

.LDIFF_SYM243=.LDIE_I4 - .Ldebug_info_start
	.long .LDIFF_SYM243
	.byte 0,0

.section ".debug_frame"
.subsection 0

.LDIFF_SYM244=.Lfde22_end - .Lfde22_start
	.long .LDIFF_SYM244
.Lfde22_start:

	.long 0
	.balign 8
	.xword .Lm_136

.LDIFF_SYM245=.Lme_136 - .Lm_136
	.long .LDIFF_SYM245
	.long 0
	.byte 12,31,0,68,14,64,157,8,158,7,68,13,29
	.balign 8
.Lfde22_end:

.section ".debug_info"
.subsection 0

	.byte 2
	.string "System.Numerics.BigInteger:System.Numerics.INumberBase<System.Numerics.BigInteger>.TryConvertToSaturating<TOther_REF>"
	.string "System_Numerics_BigInteger_System_Numerics_INumberBase_System_Numerics_BigInteger_TryConvertToSaturating_TOther_REF_System_Numerics_BigInteger_TOther_REF_"

	.byte 0,0
	.string "System.Numerics.BigInteger:System.Numerics.INumberBase<System.Numerics.BigInteger>.TryConvertToSaturating<TOther_REF>"
	.xword .Lm_137
	.xword .Lme_137

	.byte 2,118,16,3
	.string "param0"

.LDIFF_SYM246=.LDIE_I4 - .Ldebug_info_start
	.long .LDIFF_SYM246
	.byte 2,141,16,3
	.string "param1"

.LDIFF_SYM247=.LDIE_I - .Ldebug_info_start
	.long .LDIFF_SYM247
	.byte 2,141,32,11
	.string "V_0"

.LDIFF_SYM248=.LDIE_I4 - .Ldebug_info_start
	.long .LDIFF_SYM248
	.byte 0,11
	.string "V_1"

.LDIFF_SYM249=.LDIE_I4 - .Ldebug_info_start
	.long .LDIFF_SYM249
	.byte 0,11
	.string "V_2"

.LDIFF_SYM250=.LDIE_I4 - .Ldebug_info_start
	.long .LDIFF_SYM250
	.byte 0,11
	.string "V_3"

.LDIFF_SYM251=.LDIE_I4 - .Ldebug_info_start
	.long .LDIFF_SYM251
	.byte 0,11
	.string "V_4"

.LDIFF_SYM252=.LDIE_I4 - .Ldebug_info_start
	.long .LDIFF_SYM252
	.byte 0,11
	.string "V_5"

.LDIFF_SYM253=.LDIE_I4 - .Ldebug_info_start
	.long .LDIFF_SYM253
	.byte 0,11
	.string "V_6"

.LDIFF_SYM254=.LDIE_I4 - .Ldebug_info_start
	.long .LDIFF_SYM254
	.byte 0,11
	.string "V_7"

.LDIFF_SYM255=.LDIE_I4 - .Ldebug_info_start
	.long .LDIFF_SYM255
	.byte 0,11
	.string "V_8"

.LDIFF_SYM256=.LDIE_I4 - .Ldebug_info_start
	.long .LDIFF_SYM256
	.byte 0,11
	.string "V_9"

.LDIFF_SYM257=.LDIE_I4 - .Ldebug_info_start
	.long .LDIFF_SYM257
	.byte 0,11
	.string "V_10"

.LDIFF_SYM258=.LDIE_I4 - .Ldebug_info_start
	.long .LDIFF_SYM258
	.byte 0,11
	.string "V_11"

.LDIFF_SYM259=.LDIE_I4 - .Ldebug_info_start
	.long .LDIFF_SYM259
	.byte 0,11
	.string "V_12"

.LDIFF_SYM260=.LDIE_I4 - .Ldebug_info_start
	.long .LDIFF_SYM260
	.byte 0,11
	.string "V_13"

.LDIFF_SYM261=.LDIE_I4 - .Ldebug_info_start
	.long .LDIFF_SYM261
	.byte 0,11
	.string "V_14"

.LDIFF_SYM262=.LDIE_I4 - .Ldebug_info_start
	.long .LDIFF_SYM262
	.byte 0,11
	.string "V_15"

.LDIFF_SYM263=.LDIE_I4 - .Ldebug_info_start
	.long .LDIFF_SYM263
	.byte 0,11
	.string "V_16"

.LDIFF_SYM264=.LDIE_I4 - .Ldebug_info_start
	.long .LDIFF_SYM264
	.byte 0,11
	.string "V_17"

.LDIFF_SYM265=.LDIE_I4 - .Ldebug_info_start
	.long .LDIFF_SYM265
	.byte 0,0

.section ".debug_frame"
.subsection 0

.LDIFF_SYM266=.Lfde23_end - .Lfde23_start
	.long .LDIFF_SYM266
.Lfde23_start:

	.long 0
	.balign 8
	.xword .Lm_137

.LDIFF_SYM267=.Lme_137 - .Lm_137
	.long .LDIFF_SYM267
	.long 0
	.byte 12,31,0,84,14,208,4,157,74,158,73,68,13,29
	.balign 8
.Lfde23_end:

.section ".debug_info"
.subsection 0

	.byte 2
	.string "System.Numerics.BigInteger:System.Numerics.INumberBase<System.Numerics.BigInteger>.TryConvertToTruncating<TOther_REF>"
	.string "System_Numerics_BigInteger_System_Numerics_INumberBase_System_Numerics_BigInteger_TryConvertToTruncating_TOther_REF_System_Numerics_BigInteger_TOther_REF_"

	.byte 0,0
	.string "System.Numerics.BigInteger:System.Numerics.INumberBase<System.Numerics.BigInteger>.TryConvertToTruncating<TOther_REF>"
	.xword .Lm_138
	.xword .Lme_138

	.byte 2,118,16,3
	.string "param0"

.LDIFF_SYM268=.LDIE_I4 - .Ldebug_info_start
	.long .LDIFF_SYM268
	.byte 2,141,16,3
	.string "param1"

.LDIFF_SYM269=.LDIE_I - .Ldebug_info_start
	.long .LDIFF_SYM269
	.byte 2,141,32,11
	.string "V_0"

.LDIFF_SYM270=.LDIE_I4 - .Ldebug_info_start
	.long .LDIFF_SYM270
	.byte 0,11
	.string "V_1"

.LDIFF_SYM271=.LDIE_I4 - .Ldebug_info_start
	.long .LDIFF_SYM271
	.byte 0,11
	.string "V_2"

.LDIFF_SYM272=.LDIE_I4 - .Ldebug_info_start
	.long .LDIFF_SYM272
	.byte 0,11
	.string "V_3"

.LDIFF_SYM273=.LDIE_I4 - .Ldebug_info_start
	.long .LDIFF_SYM273
	.byte 0,11
	.string "V_4"

.LDIFF_SYM274=.LDIE_I4 - .Ldebug_info_start
	.long .LDIFF_SYM274
	.byte 0,11
	.string "V_5"

.LDIFF_SYM275=.LDIE_I4 - .Ldebug_info_start
	.long .LDIFF_SYM275
	.byte 0,11
	.string "V_6"

.LDIFF_SYM276=.LDIE_I4 - .Ldebug_info_start
	.long .LDIFF_SYM276
	.byte 0,11
	.string "V_7"

.LDIFF_SYM277=.LDIE_I4 - .Ldebug_info_start
	.long .LDIFF_SYM277
	.byte 0,11
	.string "V_8"

.LDIFF_SYM278=.LDIE_I4 - .Ldebug_info_start
	.long .LDIFF_SYM278
	.byte 0,11
	.string "V_9"

.LDIFF_SYM279=.LDIE_I4 - .Ldebug_info_start
	.long .LDIFF_SYM279
	.byte 0,11
	.string "V_10"

.LDIFF_SYM280=.LDIE_I4 - .Ldebug_info_start
	.long .LDIFF_SYM280
	.byte 0,11
	.string "V_11"

.LDIFF_SYM281=.LDIE_I4 - .Ldebug_info_start
	.long .LDIFF_SYM281
	.byte 0,11
	.string "V_12"

.LDIFF_SYM282=.LDIE_I4 - .Ldebug_info_start
	.long .LDIFF_SYM282
	.byte 0,11
	.string "V_13"

.LDIFF_SYM283=.LDIE_I4 - .Ldebug_info_start
	.long .LDIFF_SYM283
	.byte 0,11
	.string "V_14"

.LDIFF_SYM284=.LDIE_I4 - .Ldebug_info_start
	.long .LDIFF_SYM284
	.byte 3,141,192,0,11
	.string "V_15"

.LDIFF_SYM285=.LDIE_I4 - .Ldebug_info_start
	.long .LDIFF_SYM285
	.byte 0,11
	.string "V_16"

.LDIFF_SYM286=.LDIE_I4 - .Ldebug_info_start
	.long .LDIFF_SYM286
	.byte 0,11
	.string "V_17"

.LDIFF_SYM287=.LDIE_I4 - .Ldebug_info_start
	.long .LDIFF_SYM287
	.byte 0,11
	.string "V_18"

.LDIFF_SYM288=.LDIE_I4 - .Ldebug_info_start
	.long .LDIFF_SYM288
	.byte 0,11
	.string "V_19"

.LDIFF_SYM289=.LDIE_I4 - .Ldebug_info_start
	.long .LDIFF_SYM289
	.byte 0,11
	.string "V_20"

.LDIFF_SYM290=.LDIE_I4 - .Ldebug_info_start
	.long .LDIFF_SYM290
	.byte 0,11
	.string "V_21"

.LDIFF_SYM291=.LDIE_I4 - .Ldebug_info_start
	.long .LDIFF_SYM291
	.byte 0,11
	.string "V_22"

.LDIFF_SYM292=.LDIE_I4 - .Ldebug_info_start
	.long .LDIFF_SYM292
	.byte 0,11
	.string "V_23"

.LDIFF_SYM293=.LDIE_I4 - .Ldebug_info_start
	.long .LDIFF_SYM293
	.byte 0,11
	.string "V_24"

.LDIFF_SYM294=.LDIE_I4 - .Ldebug_info_start
	.long .LDIFF_SYM294
	.byte 0,11
	.string "V_25"

.LDIFF_SYM295=.LDIE_I4 - .Ldebug_info_start
	.long .LDIFF_SYM295
	.byte 0,11
	.string "V_26"

.LDIFF_SYM296=.LDIE_I4 - .Ldebug_info_start
	.long .LDIFF_SYM296
	.byte 0,11
	.string "V_27"

.LDIFF_SYM297=.LDIE_I4 - .Ldebug_info_start
	.long .LDIFF_SYM297
	.byte 0,11
	.string "V_28"

.LDIFF_SYM298=.LDIE_I4 - .Ldebug_info_start
	.long .LDIFF_SYM298
	.byte 0,11
	.string "V_29"

.LDIFF_SYM299=.LDIE_I4 - .Ldebug_info_start
	.long .LDIFF_SYM299
	.byte 2,141,48,11
	.string "V_30"

.LDIFF_SYM300=.LDIE_I4 - .Ldebug_info_start
	.long .LDIFF_SYM300
	.byte 0,11
	.string "V_31"

.LDIFF_SYM301=.LDIE_I4 - .Ldebug_info_start
	.long .LDIFF_SYM301
	.byte 0,0

.section ".debug_frame"
.subsection 0

.LDIFF_SYM302=.Lfde24_end - .Lfde24_start
	.long .LDIFF_SYM302
.Lfde24_start:

	.long 0
	.balign 8
	.xword .Lm_138

.LDIFF_SYM303=.Lme_138 - .Lm_138
	.long .LDIFF_SYM303
	.long 0
	.byte 12,31,0,68,14,240,3,157,62,158,61,68,13,29
	.balign 8
.Lfde24_end:

.section ".debug_info"
.subsection 0

	.byte 2
	.string "System.Numerics.Complex:CreateChecked<TOther_REF>"
	.string "System_Numerics_Complex_CreateChecked_TOther_REF_TOther_REF"

	.byte 0,0
	.string "System.Numerics.Complex:CreateChecked<TOther_REF>"
	.xword .Lm_16f
	.xword .Lme_16f

	.byte 2,118,16,3
	.string "param0"

.LDIFF_SYM304=.LDIE_I4 - .Ldebug_info_start
	.long .LDIFF_SYM304
	.byte 3,141,208,0,11
	.string "V_0"

.LDIFF_SYM305=.LDIE_I4 - .Ldebug_info_start
	.long .LDIFF_SYM305
	.byte 3,141,224,0,0

.section ".debug_frame"
.subsection 0

.LDIFF_SYM306=.Lfde25_end - .Lfde25_start
	.long .LDIFF_SYM306
.Lfde25_start:

	.long 0
	.balign 8
	.xword .Lm_16f

.LDIFF_SYM307=.Lme_16f - .Lm_16f
	.long .LDIFF_SYM307
	.long 0
	.byte 12,31,0,68,14,112,157,14,158,13,68,13,29
	.balign 8
.Lfde25_end:

.section ".debug_info"
.subsection 0

	.byte 2
	.string "System.Numerics.Complex:CreateSaturating<TOther_REF>"
	.string "System_Numerics_Complex_CreateSaturating_TOther_REF_TOther_REF"

	.byte 0,0
	.string "System.Numerics.Complex:CreateSaturating<TOther_REF>"
	.xword .Lm_170
	.xword .Lme_170

	.byte 2,118,16,3
	.string "param0"

.LDIFF_SYM308=.LDIE_I4 - .Ldebug_info_start
	.long .LDIFF_SYM308
	.byte 3,141,208,0,11
	.string "V_0"

.LDIFF_SYM309=.LDIE_I4 - .Ldebug_info_start
	.long .LDIFF_SYM309
	.byte 3,141,224,0,0

.section ".debug_frame"
.subsection 0

.LDIFF_SYM310=.Lfde26_end - .Lfde26_start
	.long .LDIFF_SYM310
.Lfde26_start:

	.long 0
	.balign 8
	.xword .Lm_170

.LDIFF_SYM311=.Lme_170 - .Lm_170
	.long .LDIFF_SYM311
	.long 0
	.byte 12,31,0,68,14,112,157,14,158,13,68,13,29
	.balign 8
.Lfde26_end:

.section ".debug_info"
.subsection 0

	.byte 2
	.string "System.Numerics.Complex:CreateTruncating<TOther_REF>"
	.string "System_Numerics_Complex_CreateTruncating_TOther_REF_TOther_REF"

	.byte 0,0
	.string "System.Numerics.Complex:CreateTruncating<TOther_REF>"
	.xword .Lm_171
	.xword .Lme_171

	.byte 2,118,16,3
	.string "param0"

.LDIFF_SYM312=.LDIE_I4 - .Ldebug_info_start
	.long .LDIFF_SYM312
	.byte 3,141,208,0,11
	.string "V_0"

.LDIFF_SYM313=.LDIE_I4 - .Ldebug_info_start
	.long .LDIFF_SYM313
	.byte 3,141,224,0,0

.section ".debug_frame"
.subsection 0

.LDIFF_SYM314=.Lfde27_end - .Lfde27_start
	.long .LDIFF_SYM314
.Lfde27_start:

	.long 0
	.balign 8
	.xword .Lm_171

.LDIFF_SYM315=.Lme_171 - .Lm_171
	.long .LDIFF_SYM315
	.long 0
	.byte 12,31,0,68,14,112,157,14,158,13,68,13,29
	.balign 8
.Lfde27_end:

.section ".debug_info"
.subsection 0
.LTDIE_14:

	.byte 5
	.string "System_Numerics_Complex"

	.byte 32,16
.LDIFF_SYM316=.LTDIE_1 - .Ldebug_info_start
	.long .LDIFF_SYM316
	.byte 2,35,0,6
	.string "m_real"

.LDIFF_SYM317=.LDIE_R8 - .Ldebug_info_start
	.long .LDIFF_SYM317
	.byte 2,35,0,6
	.string "m_imaginary"

.LDIFF_SYM318=.LDIE_R8 - .Ldebug_info_start
	.long .LDIFF_SYM318
	.byte 2,35,8,0,7
	.string "System_Numerics_Complex"

.LDIFF_SYM319=.LTDIE_14 - .Ldebug_info_start
	.long .LDIFF_SYM319
.LTDIE_14_POINTER:

	.byte 13
.LDIFF_SYM320=.LTDIE_14 - .Ldebug_info_start
	.long .LDIFF_SYM320
.LTDIE_14_REFERENCE:

	.byte 14
.LDIFF_SYM321=.LTDIE_14 - .Ldebug_info_start
	.long .LDIFF_SYM321
	.byte 2
	.string "System.Numerics.Complex:System.Numerics.INumberBase<System.Numerics.Complex>.TryConvertFromChecked<TOther_REF>"
	.string "System_Numerics_Complex_System_Numerics_INumberBase_System_Numerics_Complex_TryConvertFromChecked_TOther_REF_TOther_REF_System_Numerics_Complex_"

	.byte 0,0
	.string "System.Numerics.Complex:System.Numerics.INumberBase<System.Numerics.Complex>.TryConvertFromChecked<TOther_REF>"
	.xword .Lm_174
	.xword .Lme_174

	.byte 2,118,16,3
	.string "param0"

.LDIFF_SYM322=.LDIE_I4 - .Ldebug_info_start
	.long .LDIFF_SYM322
	.byte 2,141,16,3
	.string "param1"

.LDIFF_SYM323=.LDIE_I - .Ldebug_info_start
	.long .LDIFF_SYM323
	.byte 2,141,24,0

.section ".debug_frame"
.subsection 0

.LDIFF_SYM324=.Lfde28_end - .Lfde28_start
	.long .LDIFF_SYM324
.Lfde28_start:

	.long 0
	.balign 8
	.xword .Lm_174

.LDIFF_SYM325=.Lme_174 - .Lm_174
	.long .LDIFF_SYM325
	.long 0
	.byte 12,31,0,68,14,48,157,6,158,5,68,13,29
	.balign 8
.Lfde28_end:

.section ".debug_info"
.subsection 0

	.byte 2
	.string "System.Numerics.Complex:System.Numerics.INumberBase<System.Numerics.Complex>.TryConvertFromSaturating<TOther_REF>"
	.string "System_Numerics_Complex_System_Numerics_INumberBase_System_Numerics_Complex_TryConvertFromSaturating_TOther_REF_TOther_REF_System_Numerics_Complex_"

	.byte 0,0
	.string "System.Numerics.Complex:System.Numerics.INumberBase<System.Numerics.Complex>.TryConvertFromSaturating<TOther_REF>"
	.xword .Lm_175
	.xword .Lme_175

	.byte 2,118,16,3
	.string "param0"

.LDIFF_SYM326=.LDIE_I4 - .Ldebug_info_start
	.long .LDIFF_SYM326
	.byte 2,141,16,3
	.string "param1"

.LDIFF_SYM327=.LDIE_I - .Ldebug_info_start
	.long .LDIFF_SYM327
	.byte 2,141,24,0

.section ".debug_frame"
.subsection 0

.LDIFF_SYM328=.Lfde29_end - .Lfde29_start
	.long .LDIFF_SYM328
.Lfde29_start:

	.long 0
	.balign 8
	.xword .Lm_175

.LDIFF_SYM329=.Lme_175 - .Lm_175
	.long .LDIFF_SYM329
	.long 0
	.byte 12,31,0,68,14,48,157,6,158,5,68,13,29
	.balign 8
.Lfde29_end:

.section ".debug_info"
.subsection 0

	.byte 2
	.string "System.Numerics.Complex:System.Numerics.INumberBase<System.Numerics.Complex>.TryConvertFromTruncating<TOther_REF>"
	.string "System_Numerics_Complex_System_Numerics_INumberBase_System_Numerics_Complex_TryConvertFromTruncating_TOther_REF_TOther_REF_System_Numerics_Complex_"

	.byte 0,0
	.string "System.Numerics.Complex:System.Numerics.INumberBase<System.Numerics.Complex>.TryConvertFromTruncating<TOther_REF>"
	.xword .Lm_176
	.xword .Lme_176

	.byte 2,118,16,3
	.string "param0"

.LDIFF_SYM330=.LDIE_I4 - .Ldebug_info_start
	.long .LDIFF_SYM330
	.byte 2,141,16,3
	.string "param1"

.LDIFF_SYM331=.LDIE_I - .Ldebug_info_start
	.long .LDIFF_SYM331
	.byte 2,141,24,0

.section ".debug_frame"
.subsection 0

.LDIFF_SYM332=.Lfde30_end - .Lfde30_start
	.long .LDIFF_SYM332
.Lfde30_start:

	.long 0
	.balign 8
	.xword .Lm_176

.LDIFF_SYM333=.Lme_176 - .Lm_176
	.long .LDIFF_SYM333
	.long 0
	.byte 12,31,0,68,14,48,157,6,158,5,68,13,29
	.balign 8
.Lfde30_end:

.section ".debug_info"
.subsection 0

	.byte 2
	.string "System.Numerics.Complex:TryConvertFrom<TOther_REF>"
	.string "System_Numerics_Complex_TryConvertFrom_TOther_REF_TOther_REF_System_Numerics_Complex_"

	.byte 0,0
	.string "System.Numerics.Complex:TryConvertFrom<TOther_REF>"
	.xword .Lm_177
	.xword .Lme_177

	.byte 2,118,16,3
	.string "param0"

.LDIFF_SYM334=.LDIE_I4 - .Ldebug_info_start
	.long .LDIFF_SYM334
	.byte 0,3
	.string "param1"

.LDIFF_SYM335=.LDIE_I - .Ldebug_info_start
	.long .LDIFF_SYM335
	.byte 2,141,24,11
	.string "V_0"

.LDIFF_SYM336=.LDIE_I4 - .Ldebug_info_start
	.long .LDIFF_SYM336
	.byte 0,11
	.string "V_1"

.LDIFF_SYM337=.LDIE_I4 - .Ldebug_info_start
	.long .LDIFF_SYM337
	.byte 0,11
	.string "V_2"

.LDIFF_SYM338=.LDIE_I4 - .Ldebug_info_start
	.long .LDIFF_SYM338
	.byte 0,11
	.string "V_3"

.LDIFF_SYM339=.LDIE_I4 - .Ldebug_info_start
	.long .LDIFF_SYM339
	.byte 0,11
	.string "V_4"

.LDIFF_SYM340=.LDIE_I4 - .Ldebug_info_start
	.long .LDIFF_SYM340
	.byte 0,11
	.string "V_5"

.LDIFF_SYM341=.LDIE_I4 - .Ldebug_info_start
	.long .LDIFF_SYM341
	.byte 0,11
	.string "V_6"

.LDIFF_SYM342=.LDIE_I4 - .Ldebug_info_start
	.long .LDIFF_SYM342
	.byte 0,11
	.string "V_7"

.LDIFF_SYM343=.LDIE_I4 - .Ldebug_info_start
	.long .LDIFF_SYM343
	.byte 0,11
	.string "V_8"

.LDIFF_SYM344=.LDIE_I4 - .Ldebug_info_start
	.long .LDIFF_SYM344
	.byte 0,11
	.string "V_9"

.LDIFF_SYM345=.LDIE_I4 - .Ldebug_info_start
	.long .LDIFF_SYM345
	.byte 0,11
	.string "V_10"

.LDIFF_SYM346=.LDIE_I4 - .Ldebug_info_start
	.long .LDIFF_SYM346
	.byte 0,11
	.string "V_11"

.LDIFF_SYM347=.LDIE_I4 - .Ldebug_info_start
	.long .LDIFF_SYM347
	.byte 0,11
	.string "V_12"

.LDIFF_SYM348=.LDIE_I4 - .Ldebug_info_start
	.long .LDIFF_SYM348
	.byte 0,11
	.string "V_13"

.LDIFF_SYM349=.LDIE_I4 - .Ldebug_info_start
	.long .LDIFF_SYM349
	.byte 0,11
	.string "V_14"

.LDIFF_SYM350=.LDIE_I4 - .Ldebug_info_start
	.long .LDIFF_SYM350
	.byte 0,11
	.string "V_15"

.LDIFF_SYM351=.LDIE_I4 - .Ldebug_info_start
	.long .LDIFF_SYM351
	.byte 0,11
	.string "V_16"

.LDIFF_SYM352=.LDIE_I4 - .Ldebug_info_start
	.long .LDIFF_SYM352
	.byte 0,0

.section ".debug_frame"
.subsection 0

.LDIFF_SYM353=.Lfde31_end - .Lfde31_start
	.long .LDIFF_SYM353
.Lfde31_start:

	.long 0
	.balign 8
	.xword .Lm_177

.LDIFF_SYM354=.Lme_177 - .Lm_177
	.long .LDIFF_SYM354
	.long 0
	.byte 12,31,0,68,14,128,2,157,32,158,31,68,13,29
	.balign 8
.Lfde31_end:

.section ".debug_info"
.subsection 0

	.byte 2
	.string "System.Numerics.Complex:System.Numerics.INumberBase<System.Numerics.Complex>.TryConvertToChecked<TOther_REF>"
	.string "System_Numerics_Complex_System_Numerics_INumberBase_System_Numerics_Complex_TryConvertToChecked_TOther_REF_System_Numerics_Complex_TOther_REF_"

	.byte 0,0
	.string "System.Numerics.Complex:System.Numerics.INumberBase<System.Numerics.Complex>.TryConvertToChecked<TOther_REF>"
	.xword .Lm_178
	.xword .Lme_178

	.byte 2,118,16,3
	.string "param0"

.LDIFF_SYM355=.LDIE_I4 - .Ldebug_info_start
	.long .LDIFF_SYM355
	.byte 2,141,16,3
	.string "param1"

.LDIFF_SYM356=.LDIE_I - .Ldebug_info_start
	.long .LDIFF_SYM356
	.byte 3,141,208,0,11
	.string "V_0"

.LDIFF_SYM357=.LDIE_I4 - .Ldebug_info_start
	.long .LDIFF_SYM357
	.byte 0,11
	.string "V_1"

.LDIFF_SYM358=.LDIE_I4 - .Ldebug_info_start
	.long .LDIFF_SYM358
	.byte 0,11
	.string "V_2"

.LDIFF_SYM359=.LDIE_I4 - .Ldebug_info_start
	.long .LDIFF_SYM359
	.byte 0,11
	.string "V_3"

.LDIFF_SYM360=.LDIE_I4 - .Ldebug_info_start
	.long .LDIFF_SYM360
	.byte 0,11
	.string "V_4"

.LDIFF_SYM361=.LDIE_I4 - .Ldebug_info_start
	.long .LDIFF_SYM361
	.byte 0,11
	.string "V_5"

.LDIFF_SYM362=.LDIE_I4 - .Ldebug_info_start
	.long .LDIFF_SYM362
	.byte 0,11
	.string "V_6"

.LDIFF_SYM363=.LDIE_I4 - .Ldebug_info_start
	.long .LDIFF_SYM363
	.byte 0,11
	.string "V_7"

.LDIFF_SYM364=.LDIE_I4 - .Ldebug_info_start
	.long .LDIFF_SYM364
	.byte 0,11
	.string "V_8"

.LDIFF_SYM365=.LDIE_I4 - .Ldebug_info_start
	.long .LDIFF_SYM365
	.byte 0,11
	.string "V_9"

.LDIFF_SYM366=.LDIE_I4 - .Ldebug_info_start
	.long .LDIFF_SYM366
	.byte 0,11
	.string "V_10"

.LDIFF_SYM367=.LDIE_I4 - .Ldebug_info_start
	.long .LDIFF_SYM367
	.byte 0,11
	.string "V_11"

.LDIFF_SYM368=.LDIE_I4 - .Ldebug_info_start
	.long .LDIFF_SYM368
	.byte 0,11
	.string "V_12"

.LDIFF_SYM369=.LDIE_I4 - .Ldebug_info_start
	.long .LDIFF_SYM369
	.byte 0,11
	.string "V_13"

.LDIFF_SYM370=.LDIE_I4 - .Ldebug_info_start
	.long .LDIFF_SYM370
	.byte 0,11
	.string "V_14"

.LDIFF_SYM371=.LDIE_I4 - .Ldebug_info_start
	.long .LDIFF_SYM371
	.byte 0,11
	.string "V_15"

.LDIFF_SYM372=.LDIE_I4 - .Ldebug_info_start
	.long .LDIFF_SYM372
	.byte 0,11
	.string "V_16"

.LDIFF_SYM373=.LDIE_I4 - .Ldebug_info_start
	.long .LDIFF_SYM373
	.byte 0,11
	.string "V_17"

.LDIFF_SYM374=.LDIE_I4 - .Ldebug_info_start
	.long .LDIFF_SYM374
	.byte 0,0

.section ".debug_frame"
.subsection 0

.LDIFF_SYM375=.Lfde32_end - .Lfde32_start
	.long .LDIFF_SYM375
.Lfde32_start:

	.long 0
	.balign 8
	.xword .Lm_178

.LDIFF_SYM376=.Lme_178 - .Lm_178
	.long .LDIFF_SYM376
	.long 0
	.byte 12,31,0,68,14,144,1,157,18,158,17,68,13,29
	.balign 8
.Lfde32_end:

.section ".debug_info"
.subsection 0

	.byte 2
	.string "System.Numerics.Complex:System.Numerics.INumberBase<System.Numerics.Complex>.TryConvertToSaturating<TOther_REF>"
	.string "System_Numerics_Complex_System_Numerics_INumberBase_System_Numerics_Complex_TryConvertToSaturating_TOther_REF_System_Numerics_Complex_TOther_REF_"

	.byte 0,0
	.string "System.Numerics.Complex:System.Numerics.INumberBase<System.Numerics.Complex>.TryConvertToSaturating<TOther_REF>"
	.xword .Lm_179
	.xword .Lme_179

	.byte 2,118,16,3
	.string "param0"

.LDIFF_SYM377=.LDIE_I4 - .Ldebug_info_start
	.long .LDIFF_SYM377
	.byte 2,141,16,3
	.string "param1"

.LDIFF_SYM378=.LDIE_I - .Ldebug_info_start
	.long .LDIFF_SYM378
	.byte 3,141,208,0,11
	.string "V_0"

.LDIFF_SYM379=.LDIE_I4 - .Ldebug_info_start
	.long .LDIFF_SYM379
	.byte 0,11
	.string "V_1"

.LDIFF_SYM380=.LDIE_I4 - .Ldebug_info_start
	.long .LDIFF_SYM380
	.byte 0,11
	.string "V_2"

.LDIFF_SYM381=.LDIE_I4 - .Ldebug_info_start
	.long .LDIFF_SYM381
	.byte 0,11
	.string "V_3"

.LDIFF_SYM382=.LDIE_I4 - .Ldebug_info_start
	.long .LDIFF_SYM382
	.byte 0,11
	.string "V_4"

.LDIFF_SYM383=.LDIE_I4 - .Ldebug_info_start
	.long .LDIFF_SYM383
	.byte 0,11
	.string "V_5"

.LDIFF_SYM384=.LDIE_I4 - .Ldebug_info_start
	.long .LDIFF_SYM384
	.byte 0,11
	.string "V_6"

.LDIFF_SYM385=.LDIE_I4 - .Ldebug_info_start
	.long .LDIFF_SYM385
	.byte 0,11
	.string "V_7"

.LDIFF_SYM386=.LDIE_I4 - .Ldebug_info_start
	.long .LDIFF_SYM386
	.byte 0,11
	.string "V_8"

.LDIFF_SYM387=.LDIE_I4 - .Ldebug_info_start
	.long .LDIFF_SYM387
	.byte 0,11
	.string "V_9"

.LDIFF_SYM388=.LDIE_I4 - .Ldebug_info_start
	.long .LDIFF_SYM388
	.byte 0,11
	.string "V_10"

.LDIFF_SYM389=.LDIE_I4 - .Ldebug_info_start
	.long .LDIFF_SYM389
	.byte 0,11
	.string "V_11"

.LDIFF_SYM390=.LDIE_I4 - .Ldebug_info_start
	.long .LDIFF_SYM390
	.byte 0,11
	.string "V_12"

.LDIFF_SYM391=.LDIE_I4 - .Ldebug_info_start
	.long .LDIFF_SYM391
	.byte 0,11
	.string "V_13"

.LDIFF_SYM392=.LDIE_I4 - .Ldebug_info_start
	.long .LDIFF_SYM392
	.byte 0,11
	.string "V_14"

.LDIFF_SYM393=.LDIE_I4 - .Ldebug_info_start
	.long .LDIFF_SYM393
	.byte 0,11
	.string "V_15"

.LDIFF_SYM394=.LDIE_I4 - .Ldebug_info_start
	.long .LDIFF_SYM394
	.byte 0,11
	.string "V_16"

.LDIFF_SYM395=.LDIE_I4 - .Ldebug_info_start
	.long .LDIFF_SYM395
	.byte 0,11
	.string "V_17"

.LDIFF_SYM396=.LDIE_I4 - .Ldebug_info_start
	.long .LDIFF_SYM396
	.byte 0,0

.section ".debug_frame"
.subsection 0

.LDIFF_SYM397=.Lfde33_end - .Lfde33_start
	.long .LDIFF_SYM397
.Lfde33_start:

	.long 0
	.balign 8
	.xword .Lm_179

.LDIFF_SYM398=.Lme_179 - .Lm_179
	.long .LDIFF_SYM398
	.long 0
	.byte 12,31,0,68,14,240,1,157,30,158,29,68,13,29
	.balign 8
.Lfde33_end:

.section ".debug_info"
.subsection 0

	.byte 2
	.string "System.Numerics.Complex:System.Numerics.INumberBase<System.Numerics.Complex>.TryConvertToTruncating<TOther_REF>"
	.string "System_Numerics_Complex_System_Numerics_INumberBase_System_Numerics_Complex_TryConvertToTruncating_TOther_REF_System_Numerics_Complex_TOther_REF_"

	.byte 0,0
	.string "System.Numerics.Complex:System.Numerics.INumberBase<System.Numerics.Complex>.TryConvertToTruncating<TOther_REF>"
	.xword .Lm_17a
	.xword .Lme_17a

	.byte 2,118,16,3
	.string "param0"

.LDIFF_SYM399=.LDIE_I4 - .Ldebug_info_start
	.long .LDIFF_SYM399
	.byte 2,141,16,3
	.string "param1"

.LDIFF_SYM400=.LDIE_I - .Ldebug_info_start
	.long .LDIFF_SYM400
	.byte 3,141,208,0,11
	.string "V_0"

.LDIFF_SYM401=.LDIE_I4 - .Ldebug_info_start
	.long .LDIFF_SYM401
	.byte 0,11
	.string "V_1"

.LDIFF_SYM402=.LDIE_I4 - .Ldebug_info_start
	.long .LDIFF_SYM402
	.byte 0,11
	.string "V_2"

.LDIFF_SYM403=.LDIE_I4 - .Ldebug_info_start
	.long .LDIFF_SYM403
	.byte 0,11
	.string "V_3"

.LDIFF_SYM404=.LDIE_I4 - .Ldebug_info_start
	.long .LDIFF_SYM404
	.byte 0,11
	.string "V_4"

.LDIFF_SYM405=.LDIE_I4 - .Ldebug_info_start
	.long .LDIFF_SYM405
	.byte 0,11
	.string "V_5"

.LDIFF_SYM406=.LDIE_I4 - .Ldebug_info_start
	.long .LDIFF_SYM406
	.byte 0,11
	.string "V_6"

.LDIFF_SYM407=.LDIE_I4 - .Ldebug_info_start
	.long .LDIFF_SYM407
	.byte 0,11
	.string "V_7"

.LDIFF_SYM408=.LDIE_I4 - .Ldebug_info_start
	.long .LDIFF_SYM408
	.byte 0,11
	.string "V_8"

.LDIFF_SYM409=.LDIE_I4 - .Ldebug_info_start
	.long .LDIFF_SYM409
	.byte 0,11
	.string "V_9"

.LDIFF_SYM410=.LDIE_I4 - .Ldebug_info_start
	.long .LDIFF_SYM410
	.byte 0,11
	.string "V_10"

.LDIFF_SYM411=.LDIE_I4 - .Ldebug_info_start
	.long .LDIFF_SYM411
	.byte 0,11
	.string "V_11"

.LDIFF_SYM412=.LDIE_I4 - .Ldebug_info_start
	.long .LDIFF_SYM412
	.byte 0,11
	.string "V_12"

.LDIFF_SYM413=.LDIE_I4 - .Ldebug_info_start
	.long .LDIFF_SYM413
	.byte 0,11
	.string "V_13"

.LDIFF_SYM414=.LDIE_I4 - .Ldebug_info_start
	.long .LDIFF_SYM414
	.byte 0,11
	.string "V_14"

.LDIFF_SYM415=.LDIE_I4 - .Ldebug_info_start
	.long .LDIFF_SYM415
	.byte 0,11
	.string "V_15"

.LDIFF_SYM416=.LDIE_I4 - .Ldebug_info_start
	.long .LDIFF_SYM416
	.byte 0,11
	.string "V_16"

.LDIFF_SYM417=.LDIE_I4 - .Ldebug_info_start
	.long .LDIFF_SYM417
	.byte 0,11
	.string "V_17"

.LDIFF_SYM418=.LDIE_I4 - .Ldebug_info_start
	.long .LDIFF_SYM418
	.byte 0,0

.section ".debug_frame"
.subsection 0

.LDIFF_SYM419=.Lfde34_end - .Lfde34_start
	.long .LDIFF_SYM419
.Lfde34_start:

	.long 0
	.balign 8
	.xword .Lm_17a

.LDIFF_SYM420=.Lme_17a - .Lm_17a
	.long .LDIFF_SYM420
	.long 0
	.byte 12,31,0,68,14,240,1,157,30,158,29,68,13,29
	.balign 8
.Lfde34_end:

.section ".debug_info"
.subsection 0

	.byte 0
.Ldebug_info_end:
.section ".debug_line"
.subsection 0
.Ldebug_line_section_start:
.Ldebug_line_start:

	.long .Ldebug_line_end - . -4
	.short 2
	.long .Ldebug_line_header_end - . -4
	.byte 1,1,251,14,13,0,1,1,1,1,0,0,0,1,0,0,1
.section ".debug_line"
.subsection 0

	.byte 0
	.string "<unknown>"

	.byte 0,0,0,0
.Ldebug_line_header_end:

	.byte 0,1,1
.Ldebug_line_end:
.text 1
	.balign 8
mem_end:
