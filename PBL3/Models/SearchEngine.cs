using System;
using System.Linq;

namespace PBL3.Models
{
    public static class SearchEngine
    {
        // 1. Hàm xóa dấu Tiếng Việt (Giúp tìm kiếm "hanh vi" vẫn ra "hành vi")
        public static string RemoveDiacritics(string text)
        {
            if (string.IsNullOrWhiteSpace(text)) return "";
            text = text.ToLower().Trim();

            string charsFind = "áàảãạâấầẩẫậăắằẳẵặđéèẻẽẹêếềểễệíìỉĩịóòỏõọôốồổỗộơớờởỡợúùủũụưứừửữựýỳỷỹỵ";
            string charsRepl = "aaaaaaaaaaaaaaaaadeeeeeeeeeeeiiiiiooooooooooooooooouuuuuuuuuuuyyyyy";

            for (int i = 0; i < charsFind.Length; i++)
            {
                text = text.Replace(charsFind[i], charsRepl[i]);
            }
            return text;
        }

        // 2. Thuật toán chấm điểm độ khớp (Càng giống điểm càng cao)
        public static int CalculateScore(string target, string keyword)
        {
            if (string.IsNullOrWhiteSpace(target) || string.IsNullOrWhiteSpace(keyword)) return 0;

            string cleanTarget = RemoveDiacritics(target);
            string cleanKeyword = RemoveDiacritics(keyword);

            // Cấp độ 1: Trùng khớp hoàn toàn 100% -> Điểm tuyệt đối
            if (cleanTarget == cleanKeyword) return 100;

            // Cấp độ 2: Bắt đầu bằng từ khóa -> Điểm rất cao
            if (cleanTarget.StartsWith(cleanKeyword)) return 80;

            // Cấp độ 3: Chứa nguyên cụm từ khóa (ví dụ: "vượt đèn đỏ") -> Điểm cao
            if (cleanTarget.Contains(cleanKeyword)) return 60;

            // Cấp độ 4: Tách từ khóa ra, chứa TẤT CẢ các từ (nhưng nằm rải rác) -> Điểm vừa
            var words = cleanKeyword.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            int matchCount = words.Count(w => cleanTarget.Contains(w));

            if (matchCount == words.Length) return 40;

            // Cấp độ 5: Chứa MỘT VÀI từ trong từ khóa -> Điểm thấp (Vẫn cho hiển thị ở cuối)
            if (matchCount > 0) return 10 + matchCount;

            // Không liên quan gì
            return 0;
        }
    }
}