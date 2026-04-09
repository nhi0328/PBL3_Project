
$replacements = @(
    @{"N"="số th\? t\?"; "V"="số thứ tự"},
    @{"N"="Bi\u1EA8n"; "V"="Biến"},
    @{"N"="\uFFFDang"; "V"="đang"},
    @{"N"="\uFFFD\uFFFDng nh\?p"; "V"="đăng nhập"},
    @{"N"="N\uFFFDn"; "V"="Nên"},
    @{"N"="X\? l\? "; "V"="Xử lý "},
    @{"N"="s\u1EF1 kiện nút"; "V"="sự kiện nút"},
    @{"N"="s\u1EF1 k\u1EC7n n\uFFFDt"; "V"="sự kiện nút"},
    @{"N"="k\u1EC7n n\uFFFDt"; "V"="kiện nút"},
    @{"N"="Kh\uFFFDng t\?m th\?y"; "V"="Không tìm thấy"},
    @{"N"="th\uFFFDng tin chi ti\?t"; "V"="thông tin chi tiết"},
    @{"N"="Ph\?t ti\u1EA8n t\?"; "V"="Phạt tiền từ"},
    @{"N"="\uFFFD\?i v\?i ng\uFFFD\?i \uFFFDi\?u khi\u1EA8n"; "V"="đối với người điều khiển"},
    @{"N"="xe m\uFFFD t\uFFFD g\u1EA8n m\uFFFDy"; "V"="xe mô tô gắn máy"},
    @{"N"="mÔ tô g\u1EA8n m\uFFFDy"; "V"="mô tô, xe máy"},
    @{"N"="lu\?t"; "V"="luật"},
    @{"N"="kh\uFFFDng"; "V"="không"},
    @{"N"="\uFFFD\? Xoá"; "V"="Đã xoá"},
    @{"N"="Th\uFFFDng b\uFFFDo"; "V"="Thông báo"},
    @{"N"="tr\uFFFD\?c"; "V"="trước"},
    @{"N"="\uFFFD\u1EA8ng"; "V"="đồng"},
    @{"N"="Tr\? "; "V"="Trừ "},
    @{"N"="\uFFFDi\?m"; "V"="điểm"},
    @{"N"="b\u1EA8ng l\uFFFDi xe"; "V"="bằng lái xe"},
    @{"N"="Ti\uFFFDu \uFFFD\?"; "V"="Tiêu đề"},
    @{"N"="luật c\?"; "V"="luật cũ"},
    @{"N"="t\uFFFDn"; "V"="tên"},
    @{"N"="Th\uFFFDm m\?i"; "V"="Thêm mới"},
    @{"N"="Đã lưuu"; "V"="Đã lưu"},
    @{"N"="v\uFFFDo"; "V"="vào"},
    @{"N"="tr\u1EA8ng th\uFFFDi"; "V"="trạng thái"},
    @{"N"="t\uFFFDi kho\u1EA8n"; "V"="tài khoản"},
    @{"N"="s\u1EF1 k\u1EC7n n\uFFFDt"; "V"="sự kiện nút"},
    @{"N"="\uFFFD\? X\uFFFDc nh\u1EADn x\? l\? "; "V"="Đã xác nhận xử lý "}
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
