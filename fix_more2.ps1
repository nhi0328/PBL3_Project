
$replacements = @(
    @{"N"="Vui l\u1EA8ng"; "V"="Vui lòng"},
    @{"N"="nh\?p \uFFFD\?y \uFFFD\? th\uFFFDng tin"; "V"="nhập đầy đủ thông tin"},
    @{"N"="ch\u1EA8n \uFFFDt nh\?t m\?t"; "V"="chọn ít nhất một"},
    @{"N"="Ng\uFFFDy"; "V"="Ngày"},
    @{"N"="kh\uFFFDng h\?p l\?"; "V"="không hợp lệ"},
    @{"N"="\uFFFD\u1EA8nh d\u1EA8ng"; "V"="định dạng"},
    @{"N"="Ch\uFFFDa"; "V"="Chưa"},
    @{"N"="\uFFFD\? l\uFFFD"; "V"="Đã lưu"},
    @{"N"="l\uFFFDu"; "V"="lưu"},
    @{"N"="v\uFFFDo c\uFFFD"; "V"="vào cơ"},
    @{"N"="Kh\uFFFDng th\?"; "V"="Không thể"},
    @{"N"="h\u1EA8nh \u1EA8nh"; "V"="hình ảnh"},
    @{"N"="l\uFFFDn"; "V"="lên"},
    @{"N"="T\?i \uFFFD\uFFFD"; "V"="Tối đa"},
    @{"N"="T\?i \uFFFD\uFFFD"; "V"="Tối đa"},
    @{"N"="Nh\u1EA8n \uFFFD\? ch\u1EA8n Lỗi \u1EA8nh"; "V"="Nhấn để chọn lại ảnh"},
    @{"N"="Xe m\uFFFDy"; "V"="Xe máy"},
    @{"N"="Ph\uFFFD\uFFFDng ti\u1EA8n"; "V"="Phương tiện"},
    @{"N"="th\uFFFDnh c\uFFFDng"; "V"="thành công"},
    @{"N"="\uFFFD\uFFFDĐà Nẵng"; "V"="Đà Nẵng"},
    @{"N"="\uFFFDang ho\?t \uFFFD\u1EA8ng"; "V"="Đang hoạt động"},
    @{"N"="ho\?t \uFFFD\u1EA8ng"; "V"="hoạt động"},
    @{"N"="Kh\uFFFDng th\?i h\u1EA8n"; "V"="Không thời hạn"},
    @{"N"="\uFFFDang \uFFFD\uFFFD\?c"; "V"="đang được"},
    @{"N"="B\u1EA8n c\uFFFD"; "V"="Bạn có"},
    @{"N"="ch\?c ch\u1EA8n"; "V"="chắc chắn"},
    @{"N"="mu\u1EA8n xo\uFFFD"; "V"="muốn xoá"},
    @{"N"="X\uFFFDc nh\u1EA8n"; "V"="Xác nhận"},
    @{"N"="Xo\uFFFD"; "V"="Xoá"},
    @{"N"="T\?i \uFFFDa 5 \u1EA8nh"; "V"="Tối đa 5 ảnh"},
    @{"N"="T\?i \uFFFDa 5 \?nh"; "V"="Tối đa 5 ảnh"},
    @{"N"="\uFFFD\?a \?i\?m"; "V"="địa điểm"}
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
