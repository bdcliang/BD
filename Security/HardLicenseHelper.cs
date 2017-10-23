namespace BD.Security
{
using System;
using System.Diagnostics;
using System.Windows.Forms;
using app = System.Windows.Forms.Application;
using env = System.Environment;
/// <summary>
/// Encrypt DOG License Helper class
/// </summary>
public class HardLicenseHelper
 {
            /// <summary>
            /// 默认的验证,在RELEASE宏模式下有效
            /// </summary>
            [Conditional("RELEASE")]
            public static void DefaultValidate()
            {
                if (!Validate()) env.Exit(0);
            }
            /// <summary>
            /// 加密狗Key验证,MD5验证，程序中存储的ID的MD5
            /// </summary>
            /// <param name="softKey">加密狗ID 的MD5</param>
            /// <returns></returns>
            public static bool ConfirmKey(string softKey)
            {
                string id = DogKeyHelper.GetKey();
                if (id == "") return false;
                if (softKey != DogKeyHelper.StrMD5(id))
                {
                    MessageBox.Show("友情提示：此软件未获得使用授权，加密狗ID验证失败！", "授权验证失败", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    app.Exit();
                    return false;
                }
                return true;
            }
            /// <summary>
            /// 设置到期日期格式 2016-7-20
            /// </summary>
            /// <param name="date"></param>
            public static void SetExirationDate(string date)
            {
                string[] args = date.Split('-');
                if (args.Length != 3)
                    return;
                int year = int.Parse(args[0]);
                int month = int.Parse(args[1]);
                int day = int.Parse(args[2]);
                if (month > 12 || day > 31)
                    return;
                DateTime setDate = new DateTime(year, month, day);
                var tmp = setDate - DateTime.Now;
                int duration = tmp.Days;
                Console.WriteLine("Left days" + duration);
                string newString = DateTime.Now.ToString("yyyy-MM-dd") + "#" + duration.ToString();
                DogKeyHelper.WriteString(newString);
                MessageBox.Show("设置成功");
            }
            /// <summary>
            /// 获取剩余有效时间
            /// </summary>
            /// <returns></returns>
            public static string GetLeftDays()
            {
                string softDate = DogKeyHelper.ReadString();
                if (softDate.Length == 255)
                {
                    MessageBox.Show("友情提示：此软件未获得使用授权，加密狗验证失败！", "授权验证失败", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return "";
                }
                if (softDate == "")
                {
                    return "";
                }
                if (softDate == "null")
                {
                    return "";
                }
                string[] strs = softDate.Split('#');
                int duration = int.Parse(strs[1]);
                try
                {
                    if (duration>=25550) return "已终身授权";
                    return duration.ToString();
                }
                catch
                {
                    return duration.ToString();
                }

            }
            /// <summary>
            /// 终身授权
            /// </summary>
            public static void Authorize()
            {
                string softDate = DogKeyHelper.ReadString();
                string[] strs = softDate.Split('#');
                if (strs.Length >=2)
                {
                    string newString = strs[0] + "#" + "25550";
                    DogKeyHelper.WriteString(newString);
                    MessageBox.Show("终身授权成功");
                }

            }
            /// <summary>
            /// 授权一定的时间
            /// </summary>
            /// <param name="days"></param>
            public static void Authorize(int days)
            {
                string softDate = DogKeyHelper.ReadString();
                string[] strs = softDate.Split('#');
                if (strs.Length >= 2)
                {
                    string newString = strs[0] + "#" + days;
                    DogKeyHelper.WriteString(newString);
                    MessageBox.Show("授权成功");
                }
            }
            /// <summary>
            /// 软件使用日期验证
            /// </summary>
            /// <returns></returns>
            public static bool Validate()
            {
                string softDate = DogKeyHelper.ReadString();
                if (softDate.Length == 255)
                {
                    MessageBox.Show("友情提示：此软件未获得使用授权，加密狗验证失败！", "授权验证失败", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return false;
                }
                if (softDate == "")
                {
                    return false;
                }
                if (softDate == "null")
                {
                    return false;
                }
                string[] strs = softDate.Split('#');
                try
                {
                    int duration = int.Parse(strs[1]);
                    DateTime sDate = DateTime.Parse(strs[0]).Date;
                    DateTime eDate = sDate.AddDays(duration).Date;
                    if (DateTime.Now.Date < sDate)
                    {
                        MessageBox.Show("系统时间不正确，请将系统调整到正确的时间");
                        return false;
                    }

                    if (DateTime.Now.Date >= sDate && DateTime.Now.Date < eDate)
                    {
                        duration = (eDate - DateTime.Now.Date).Days;
                        string newString = DateTime.Now.ToString("yyyy-MM-dd") + "#" + duration.ToString();
                        DogKeyHelper.WriteString(newString);
                        return true;
                    }
                    else
                    {
                        MessageBox.Show("友情提示：您的软件使用授权码已过期，请获取新的授权码！", "过期提示", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        return false;
                    }
                }
                catch
                {
                    MessageBox.Show("软件使用授权码格式错误！", "格式错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return false;
                }
            }
        }
}
