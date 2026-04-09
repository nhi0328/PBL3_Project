
$replacements = @(
    @{"N"="Ch\?c n\x{FFFD}ng th\x{FFFD}m GPLX \x{FFFD}ang \x{FFFD}\x{FFFD}\?c c\?p nh\?t\."; "V"="Chức năng thêm GPLX đang được cập nhật."},
    @{"N"="B\?n c\x{FFFD} ch\?c ch\?n mu\?n xo\x{FFFD} GPLX"; "V"="Bạn có chắc chắn muốn xoá GPLX"},
    @{"N"="X\x{FFFD}c nh\?n"; "V"="Xác nhận"},
    @{"N"="Lỗi khi xo\x{FFFD} GPLX:"; "V"="Lỗi khi xoá GPLX:"},
    @{"N"="B\?n c\x{FFFD} ch\?c"; "V"="Bạn có chắc"},
    @{"N"="Kh\x{FFFD}ng th\?i h\?n"; "V"="Không thời hạn"},
    @{"N"="\x{FFFD}\x{FFFD} N\?ng"; "V"="Đà Nẵng"},
    @{"N"="t\x{FFFD}\x{FFFD}ng \x{FFFD}ng"; "V"="tương ứng"},
    @{"N"="\x{FFFD}ang ho\?t \x{FFFD}\?ng"; "V"="Đang hoạt động"},
    @{"N"="ho\?t \x{FFFD}\?ng"; "V"="hoạt động"},
    @{"N"="h\?t h\?n"; "V"="hết hạn"},
    @{"N"="H\?t h\?n"; "V"="Hết hạn"},
    @{"N"="Lỗi t\?i chi ti\?t t\x{FFFD}\x{FFFD}\?n: "; "V"="Lỗi tải chi tiết tài khoản: "},
    @{"N"="t\x{FFFD}\x{FFFD}\?n"; "V"="tài khoản"},
    @{"N"="t\x{FFFD}i kho\?n: "; "V"="tài khoản: "},
    @{"N"="Vui l\x{FFFD}ng nh\?p m\?t kh\?u m\?i\."; "V"="Vui lòng nhập mật khẩu mới."},
    @{"N"="C\?p nh\?t m\?t kh\?u th\x{FFFD}nh c\x{FFFD}ng!"; "V"="Cập nhật mật khẩu thành công!"},
    @{"N"="Lỗi c\?p nh\?t m\?t kh\?u: "; "V"="Lỗi cập nhật mật khẩu: "}
)

$files = Get-ChildItem PBL3\Page*.xaml.cs
foreach ($file in $files) {
    if ($file.Length -gt 0) {
        $filePath = $file.FullName
        $content = [System.IO.File]::ReadAllText($filePath, [System.Text.Encoding]::UTF8)
        $changed = $false
        foreach ($item in $replacements) {
            $pattern = $item["N"]
            if ($content -match $pattern) {
                $content = $content -replace $pattern, $item["V"]
                $changed = $true
            }
        }
        if ($changed) {
            [System.IO.File]::WriteAllText($filePath, $content, [System.Text.Encoding]::UTF8)
        }
    }
}
