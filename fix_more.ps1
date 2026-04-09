
$replacements = @(
    @{"N"="Ng\uFFFDy vi ph\uFFFDm kh\uFFFDng h\?p l\?\. Vui l\?ng nh\?p theo \uFFFD\?nh d\?ng"; "V"="Ngày vi phạm không hợp lệ. Vui lòng nhập theo định dạng"},
    @{"N"="Gi\? vi ph\uFFFDm kh\uFFFDng h\?p l\?\. Vui l\?ng nh\?p theo \uFFFD\?nh d\?ng"; "V"="Giờ vi phạm không hợp lệ. Vui lòng nhập theo định dạng"},
    @{"N"="Ch\uFFFDa c\?p nh\?t"; "V"="Chưa cập nhật"},
    @{"N"="\uFFFD\? l\uFFFDu vi ph\uFFFDm v\uFFFDo c\uFFFD số dữ liệu\."; "V"="Đã lưu vi phạm vào cơ sở dữ liệu."},
    @{"N"="Kh\uFFFDng th\? l\uFFFDu vi ph\uFFFDm: "; "V"="Không thể lưu vi phạm: "},
    @{"N"="Ch\?n \?nh vi ph\uFFFDm"; "V"="Chọn ảnh vi phạm"},
    @{"N"="\+ T\?i h\?nh \?nh l\uFFFDn"; "V"="+ Tải hình ảnh lên"},
    @{"N"="T\?i \uFFFDa 5 \?nh"; "V"="Tối đa 5 ảnh"},
    @{"N"="Nh\?n \uFFFD\? ch\?n l\?i \?nh"; "V"="Nhấn để chọn lại ảnh"},
    @{"N"="Xe t\?i"; "V"="Xe tải"},
    @{"N"="\uFFFD t\uFFFD"; "V"="Ô tô"},
    @{"N"="\uFFFDD"; "V"="Ô"},
    @{"N"="Ph\uFFFD\uFFFDng ti\?n"; "V"="Phương tiện"},
    @{"N"="Lỗi t\?i chi ti\?t t\uFFFDi kho\?n"; "V"="Lỗi tải chi tiết tài khoản"},
    @{"N"="Vui l\?ng nh\?p m\?t kh\?u m\?i\."; "V"="Vui lòng nhập mật khẩu mới."},
    @{"N"="C\?p nh\?t m\?t kh\?u th\uFFFDnh c\uFFFDng!"; "V"="Cập nhật mật khẩu thành công!"},
    @{"N"="\uFFFDang ho\?t \uFFFD\?ng"; "V"="Đang hoạt động"},
    @{"N"="ho\?t \uFFFD\?ng"; "V"="hoạt động"},
    @{"N"="h\?t h\?n"; "V"="hết hạn"},
    @{"N"="H\?t h\?n"; "V"="Hết hạn"},
    @{"N"="Kh\uFFFDng th\?i h\?n"; "V"="Không thời hạn"},
    @{"N"="Ch\?c n\uFFFDng th\uFFFDm GPLX"; "V"="Chức năng thêm GPLX"},
    @{"N"="B\?n c\uFFFD ch\?c ch\?n mu\?n xo\uFFFD GPLX"; "V"="Bạn có chắc chắn muốn xoá GPLX"},
    @{"N"="X\uFFFDc nh\?n"; "V"="Xác nhận"},
    @{"N"="\?n"; "V"="Ẩn"},
    @{"N"="k\? \?"; "V"="kẻ phụ"},
    @{"N"="T\?t c\?"; "V"="Tất cả"},
    @{"N"="`"L\?i`""; "V"="`"Lỗi`""},
    @{"N"="L\?i"; "V"="Lỗi"},
    @{"N"="giao di\?n"; "V"="giao diện"},
    @{"N"="Ph\?n \uFFFDnh"; "V"="Phản ánh"},
    @{"N"="ph\?n \uFFFDnh"; "V"="phản ánh"},
    @{"N"="Bi\?n b\uFFFDo"; "V"="Biển báo"},
    @{"N"="c\?p nh\?t"; "V"="cập nhật"}
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
