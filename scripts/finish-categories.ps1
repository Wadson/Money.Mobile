$p='Money/Services/DatabaseService.cs'; $s=[IO.File]::ReadAllText((Join-Path $PWD $p))
$s=$s.Replace('long? supplierId = null, long? cardId = null) => QueryAsync(', 'long? supplierId = null, long? cardId = null, long? mainCategoryId = null) => QueryAsync(')
$s=$s.Replace('long? supplierId = null, long? cardId = null, long? categoryId = null)', 'long? supplierId = null, long? cardId = null, long? categoryId = null, long? mainCategoryId = null)')
foreach($method in @('GetFinancialReportItemsAsync','GetContasPagarAsync')) {
 $start=$s.IndexOf(' '+$method+'('); $end=$s.IndexOf('public ', $start+15); $part=$s.Substring($start,$end-$start)
 $part=$part.Replace('AND (@category IS NULL OR t.id_categoria=@category)', 'AND (@category IS NULL OR t.id_categoria=@category) AND (@main IS NULL OR c.id_categoria_pilar=@main)')
 $part=$part.Replace('("@category", categoryId)', '("@main", mainCategoryId), ("@category", categoryId)')
 $part=$part.Replace('c.nome_categoria,', "(SELECT p.nome FROM CategoriaPilar p WHERE p.id_categoria_pilar=c.id_categoria_pilar AND p.id_usuario=c.id_usuario) || ' / ' || c.nome_categoria,")
 $s=$s.Substring(0,$start)+$part+$s.Substring($end)
}
[IO.File]::WriteAllText((Join-Path $PWD $p),$s)
$p='Money/Services/ReportPdfService.cs'; $s=[IO.File]::ReadAllText((Join-Path $PWD $p)).Replace('string? supplierName = null, long? cardId = null, string? cardName = null)', 'string? supplierName = null, long? cardId = null, string? cardName = null, long? mainCategoryId = null)').Replace('month, year, categoryId, paymentStatus, supplierId, cardId);', 'month, year, categoryId, paymentStatus, supplierId, cardId, mainCategoryId);'); [IO.File]::WriteAllText((Join-Path $PWD $p),$s)
