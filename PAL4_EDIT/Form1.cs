using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Runtime.InteropServices;

namespace PAL4_EDIT
{
    public partial class Form1 : Form
    {
        IntPtr  window_hwnd = new IntPtr(0);
        Int32   process_id = 0;
        IntPtr  process_handle = new IntPtr(0);
        bool is_fight = false;//是否正在战斗

        public const int YunTianHe = 0;
        public const int HanLingSha = 1;
        public const int LiuMengLi = 2;
        public const int MuRongZiYing = 3;

        int OffsetValue = 0x3E30;
        int FightOffsetValue = 0x2CC;

        public struct data_mn
        {
            public UInt16 max;
            public UInt16 now;
        }
        //角色数据
        public struct USER_DATA
        {
            public data_mn hp;  //精
            public data_mn mp;  //神
            public data_mn rage;//气
            public UInt16 wu;    //武
            public UInt16 fang;  //防
            public UInt16 su;    //速
            public UInt16 yun;   //运
            public UInt16 ling;  //灵
            public UInt16 wuFinal;    //武 加成后（显示）
            public UInt16 fangFinal;  //防
            public UInt16 suFinal;    //速
            public UInt16 yunFinal;   //运
            public UInt16 lingFinal;  //灵
            public Int16 pos;   //在队伍中的位置
            public bool inTeam;
        }
        //战斗时的用户数据
        public struct FIGHT_USER_DATA
        {
            public data_mn hp;
            public data_mn mp;
            public data_mn rage;
            public bool inTeam;//这个位置是否有人
        }

        USER_DATA[] ALL_USER;
        FIGHT_USER_DATA[] FIGHT_USER;
        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            comboBox1.SelectedIndex = 1;
            ALL_USER = new USER_DATA[4];
            FIGHT_USER = new FIGHT_USER_DATA[3];
        }
        //读2字节内存
        private bool Read2Byte(IntPtr hwnd, int hAddr, out UInt16 outData)
        {
            byte[] out_byte = new byte[2];
            int ip = new int();
            int o_sta = W_API.ReadProcessMemory(hwnd, hAddr,  out_byte, 2, out ip);
            if (o_sta == 0)
            {
                outData = 0;
                return false;
            }
            UInt16 temp = 0;
            temp |= out_byte[0];
            temp |= (UInt16)(out_byte[1] << 8);
            outData = (UInt16)temp;
            return true;
        }
        //读4字节内存
        private bool Read4Byte(IntPtr hwnd, int hAddr, out Int32 outData)
        {
            byte[] out_byte = new byte[10];
            
            int ip = new int();
            int o_sta = W_API.ReadProcessMemory(hwnd, hAddr, out_byte, 4, out ip);
            if (o_sta == 0)
            {
                outData = 0;
                return false;
            }
            UInt32 temp = 0;
            temp |= (UInt32)out_byte[0];
            temp |= ((UInt32)out_byte[1]) << 8;
            temp |= ((UInt32)out_byte[2]) << 16;
            temp |= ((UInt32)out_byte[3]) << 24;
            outData = (Int32)temp;
            return true;
        }
        //写2字节内存
        private bool Write2Byte(IntPtr hwnd, int hAddr, UInt16 inData)
        {
            byte[] temp_byte = new byte[2];
            temp_byte[0] = (byte)inData;
            temp_byte[1] = (byte)(inData >> 8);
            int ip = 0;
            int o_sta = W_API.WriteProcessMemory(hwnd, hAddr, temp_byte, 2, out ip);
            if (o_sta == 0)
            {
                return false;
            }
            return true;
        }
        //写4字节内存
        private bool Write4Byte(IntPtr hwnd, int hAddr, Int32 inData)
        {
            byte[] temp_byte = new byte[4];
            temp_byte[0] = (byte)inData;
            temp_byte[1] = (byte)(inData >> 8);
            temp_byte[2] = (byte)(inData >> 16);
            temp_byte[3] = (byte)(inData >> 24);
            int ip = 0;
            int o_sta = W_API.WriteProcessMemory(hwnd, hAddr, temp_byte, 4, out ip);
            if (o_sta == 0)
            {
                return false;
            }
            return true;
        }
        //获得战斗状态
        private void Get_Fight_Status()
        {
            Int32 out_data = 0;
            if (Read4Byte(process_handle, 0x8F3190 + OffsetValue, out out_data) == false)
            {
                is_fight = false;
                Text = "仙剑4内存修改器 - PAL4.exe - " + process_id.ToString() + " - " + "战斗状态错误";
                return;
            }

            if(is_fight == false && out_data != 0) {
                nFightInFight();
            }

            if (out_data == 0) is_fight = false;
            else is_fight = true;
            Text = "仙剑4内存修改器 - PAL4.exe - " + process_id.ToString() + (is_fight==true?" - 战斗中":"");
            //Text = "仙剑4内存修改器 - PAL4.exe - " + process_id.ToString() + " - " + out_data.ToString("X");
        }
        //获取金钱
        private Int32 Get_Money()
        {
            Int32 out_data = 0;
            if(Read4Byte(process_handle, 0x8EB064 + OffsetValue, out out_data) == false)
            {
                return 0;
            }
            
            if (Read4Byte(process_handle, out_data + 0x134, out out_data) == false)
            {
                return 0;
            }
            //MessageBox.Show("Money:" + out_data.ToString());
            return out_data;
        }
        //设置金钱
        private bool Set_Money(Int32 Money)
        {
            Int32 out_data = 0;

            if (Read4Byte(process_handle, 0x8EB064 + OffsetValue, out out_data) == false)
            {
                MessageBox.Show("读取失败");
                return false;
            }

            if (Write4Byte(process_handle, out_data + 0x134,Money) == false)
            {
                MessageBox.Show("写入失败" + W_API.GetLastError().ToString());
                return false;
            }
            MessageBox.Show("写入成功");
            return true;
        }
        //获取人物在队伍中的位置
        private Int16 Get_Pos(int id)
        {
            Int32 out_data = 0;
            if (Read4Byte(process_handle, 0x8E1428 + OffsetValue, out out_data) == false)
                return -1;
            UInt16 pos;
            if (Read2Byte(process_handle, out_data + 0x7A0 + id * 0xb14, out pos) == false)
                return -1;
            return (Int16)pos;
        }
        private void Show_Data()
        {
            try
            {
                for (int id = 0; id < 4; id++)
                {
                    string displayPos = ALL_USER[id].pos.ToString();
                    string displayHPText = ALL_USER[id].hp.now.ToString() + "/" + ALL_USER[id].hp.max.ToString();
                    string displayMPText = ALL_USER[id].mp.now.ToString() + "/" + ALL_USER[id].mp.max.ToString();
                    string displayWuText = "武：" + ALL_USER[id].wu.ToString() + "(" + ALL_USER[id].wuFinal.ToString() + ")";
                    string displayFangText = "防：" + ALL_USER[id].fang.ToString() + "(" + ALL_USER[id].fangFinal.ToString() + ")";
                    string displaySuText = "速：" + ALL_USER[id].su.ToString() + "(" + ALL_USER[id].suFinal.ToString() + ")";
                    string displayYunText = "运：" + ALL_USER[id].yun.ToString() + "(" + ALL_USER[id].yunFinal.ToString() + ")";
                    string displayLingText = "灵：" + ALL_USER[id].ling.ToString() + "(" + ALL_USER[id].lingFinal.ToString() + ")";
                    switch (id)
                    {
                        case YunTianHe://云天河
                            Y_G_T.Text = "云天河-" + displayPos;

                            Y_J.Maximum = ALL_USER[id].hp.max;
                            Y_J.Value = ALL_USER[id].hp.now;
                            Y_J_L.Text = displayHPText;

                            Y_S.Maximum = ALL_USER[id].mp.max;
                            Y_S.Value = ALL_USER[id].mp.now;
                            Y_S_L.Text = displayMPText;

                            Y_Q.Maximum = 100;
                            Y_Q.Value = ALL_USER[id].rage.now;
                            Y_Q_L.Text = ALL_USER[id].rage.now.ToString() + "/100";

                            D_Y_W.Text = displayWuText;
                            D_Y_F.Text = displayFangText;
                            D_Y_S.Text = displaySuText;
                            D_Y_Y.Text = displayYunText;
                            D_Y_L.Text = displayLingText;

                            Y_I_T.Checked = ALL_USER[id].inTeam;
                            break;
                        case HanLingSha://韩菱纱
                            H_G_T.Text = "韩菱纱-" + displayPos;

                            H_J.Maximum = ALL_USER[id].hp.max;
                            H_J.Value = ALL_USER[id].hp.now;
                            H_J_L.Text = displayHPText;

                            H_S.Maximum = ALL_USER[id].mp.max;
                            H_S.Value = ALL_USER[id].mp.now;
                            H_S_L.Text = displayMPText;

                            H_Q.Maximum = 100;
                            H_Q.Value = ALL_USER[id].rage.now;
                            H_Q_L.Text = ALL_USER[id].rage.now.ToString() + "/100";

                            D_H_W.Text = displayWuText;
                            D_H_F.Text = displayFangText;
                            D_H_S.Text = displaySuText;
                            D_H_Y.Text = displayYunText;
                            D_H_L.Text = displayLingText;

                            H_I_T.Checked = ALL_USER[id].inTeam;
                            break;
                        case LiuMengLi://柳梦璃
                            L_G_T.Text = "柳梦璃-" + displayPos;

                            L_J.Maximum = ALL_USER[id].hp.max;
                            L_J.Value = ALL_USER[id].hp.now;
                            L_J_L.Text = displayHPText;

                            L_S.Maximum = ALL_USER[id].mp.max;
                            L_S.Value = ALL_USER[id].mp.now;
                            L_S_L.Text = displayMPText;

                            L_Q.Maximum = 100;
                            L_Q.Value = ALL_USER[id].rage.now;
                            L_Q_L.Text = ALL_USER[id].rage.now.ToString() + "/100";

                            D_L_W.Text = displayWuText;
                            D_L_F.Text = displayFangText;
                            D_L_S.Text = displaySuText;
                            D_L_Y.Text = displayYunText;
                            D_L_L.Text = displayLingText;

                            L_I_T.Checked = ALL_USER[id].inTeam;
                            break;
                        case MuRongZiYing://慕容紫英
                            M_G_T.Text = "慕容紫英-" + displayPos;

                            M_J.Maximum = ALL_USER[id].hp.max;
                            M_J.Value = ALL_USER[id].hp.now;
                            M_J_L.Text = displayHPText;

                            M_S.Maximum = ALL_USER[id].mp.max;
                            M_S.Value = ALL_USER[id].mp.now;
                            M_S_L.Text = displayMPText;

                            M_Q.Maximum = 100;
                            M_Q.Value = ALL_USER[id].rage.now;
                            M_Q_L.Text = ALL_USER[id].rage.now.ToString() + "/100";

                            D_M_W.Text = displayWuText;
                            D_M_F.Text = displayFangText;
                            D_M_S.Text = displaySuText;
                            D_M_Y.Text = displayYunText;
                            D_M_L.Text = displayLingText;

                            M_I_T.Checked = ALL_USER[id].inTeam;
                            break;
                    }
                }

                for (int i = 0; i < 3; i++)
                {
                    switch (i)
                    {
                        case 0://左侧
                            if (FIGHT_USER[i].inTeam == false)
                            {
                                L_U_G.Visible = false;
                                break;
                            }
                            L_U_G.Visible = true;
                            L_F_J_L.Text = FIGHT_USER[i].hp.now.ToString() + "/" + FIGHT_USER[i].hp.max.ToString();
                            L_F_J.Maximum = FIGHT_USER[i].hp.max;
                            L_F_J.Value = FIGHT_USER[i].hp.now;

                            L_F_Q_L.Text = FIGHT_USER[i].rage.now.ToString() + "/" + FIGHT_USER[i].rage.max.ToString();
                            L_F_Q.Maximum = FIGHT_USER[i].rage.max;
                            L_F_Q.Value = FIGHT_USER[i].rage.now;

                            L_F_S_L.Text = FIGHT_USER[i].mp.now.ToString() + "/" + FIGHT_USER[i].mp.max.ToString();
                            L_F_S.Maximum = FIGHT_USER[i].mp.max;
                            L_F_S.Value = FIGHT_USER[i].mp.now;
                            break;
                        case 1://中间
                            if (FIGHT_USER[i].inTeam == false)
                            {
                                M_U_G.Visible = false;
                                break;
                            }
                            M_U_G.Visible = true;
                            M_F_J_L.Text = FIGHT_USER[i].hp.now.ToString() + "/" + FIGHT_USER[i].hp.max.ToString();
                            M_F_J.Maximum = FIGHT_USER[i].hp.max;
                            M_F_J.Value = FIGHT_USER[i].hp.now;

                            M_F_Q_L.Text = FIGHT_USER[i].rage.now.ToString() + "/" + FIGHT_USER[i].rage.max.ToString();
                            M_F_Q.Maximum = FIGHT_USER[i].rage.max;
                            M_F_Q.Value = FIGHT_USER[i].rage.now;

                            M_F_S_L.Text = FIGHT_USER[i].mp.now.ToString() + "/" + FIGHT_USER[i].mp.max.ToString();
                            M_F_S.Maximum = FIGHT_USER[i].mp.max;
                            M_F_S.Value = FIGHT_USER[i].mp.now;
                            break;
                        case 2://右侧
                            if (FIGHT_USER[i].inTeam == false)
                            {
                                R_U_G.Visible = false;
                                break;
                            }
                            R_U_G.Visible = true;
                            R_F_J_L.Text = FIGHT_USER[i].hp.now.ToString() + "/" + FIGHT_USER[i].hp.max.ToString();
                            R_F_J.Maximum = FIGHT_USER[i].hp.max;
                            R_F_J.Value = FIGHT_USER[i].hp.now;

                            R_F_Q_L.Text = FIGHT_USER[i].rage.now.ToString() + "/" + FIGHT_USER[i].rage.max.ToString();
                            R_F_Q.Maximum = FIGHT_USER[i].rage.max;
                            R_F_Q.Value = FIGHT_USER[i].rage.now;

                            R_F_S_L.Text = FIGHT_USER[i].mp.now.ToString() + "/" + FIGHT_USER[i].mp.max.ToString();
                            R_F_S.Maximum = FIGHT_USER[i].mp.max;
                            R_F_S.Value = FIGHT_USER[i].mp.now;
                            break;
                    }
                }
            }
            catch (Exception)
            {
            }
            
        }

        //获得HP MP RAGE(非战斗时)
        private void Get_HMR()
        {

            Int32 out_data = 0;
            Int32 BASE_ADDR = 0;
            if (Read4Byte(process_handle, 0x8E1428 + OffsetValue, out out_data) == false)
            {
                return;
            }
            BASE_ADDR = out_data;
            for (int id=0;id<4;id++)
            {
                UInt16 hp_now = 0;
                UInt16 hp_max = 0;
                UInt16 mp_now = 0;
                UInt16 mp_max = 0;
                UInt16 rage = 0;
                UInt16 wu = 0;
                UInt16 fang = 0;
                UInt16 su = 0;
                UInt16 yun = 0;
                UInt16 ling = 0;
                UInt16 isteam = 0;
                if (Read2Byte(process_handle, BASE_ADDR + 0x890 + id * 0xb14, out ALL_USER[id].hp.now) == false)
                    return; 
                if (Read2Byte(process_handle, BASE_ADDR + 0x7ac + id * 0xb14, out ALL_USER[id].hp.max) == false)
                    return;

                if (Read2Byte(process_handle, BASE_ADDR + 0x898 + id * 0xb14, out ALL_USER[id].mp.now) == false)
                    return;
                if (Read2Byte(process_handle, BASE_ADDR + 0x7b4 + id * 0xb14, out ALL_USER[id].mp.max) == false)
                    return;

                if (Read2Byte(process_handle, BASE_ADDR + 0x894 + id * 0xb14, out ALL_USER[id].rage.now) == false)
                    return;

                if (Read2Byte(process_handle, BASE_ADDR + 0x668 + id * 0xb14, out ALL_USER[id].wu) == false)
                    return;
                if (Read2Byte(process_handle, BASE_ADDR + 0x66c + id * 0xb14, out ALL_USER[id].fang) == false)
                    return;
                if (Read2Byte(process_handle, BASE_ADDR + 0x670 + id * 0xb14, out ALL_USER[id].su) == false)
                    return;
                if (Read2Byte(process_handle, BASE_ADDR + 0x674 + id * 0xb14, out ALL_USER[id].yun) == false)
                    return;
                if (Read2Byte(process_handle, BASE_ADDR + 0x678 + id * 0xb14, out ALL_USER[id].ling) == false)
                    return;


                if (Read2Byte(process_handle, BASE_ADDR + 0x7b8 + id * 0xb14, out ALL_USER[id].wuFinal) == false)
                    return;
                if (Read2Byte(process_handle, BASE_ADDR + 0x7bc + id * 0xb14, out ALL_USER[id].fangFinal) == false)
                    return;
                if (Read2Byte(process_handle, BASE_ADDR + 0x7c0 + id * 0xb14, out ALL_USER[id].suFinal) == false)
                    return;
                if (Read2Byte(process_handle, BASE_ADDR + 0x7c4 + id * 0xb14, out ALL_USER[id].yunFinal) == false)
                    return;
                if (Read2Byte(process_handle, BASE_ADDR + 0x7c8 + id * 0xb14, out ALL_USER[id].lingFinal) == false)
                    return;

                if (Read2Byte(process_handle, BASE_ADDR + 0xB08 + id * 0xb14, out isteam) == false)
                    return;

                ALL_USER[id].pos = Get_Pos(id);

                if (isteam == 1) ALL_USER[id].inTeam = true;
                else ALL_USER[id].inTeam = false;
            }
            //MessageBox.Show("HP:" + hp.ToString());
        }
        //锁定数据处理
        private void Set_HMR()
        {
            Int32 BASE_ADDR = 0;
            if (Read4Byte(process_handle, 0x8E1428 + OffsetValue, out BASE_ADDR) == false)
                return;

            if (Y_J_S.Checked)
                Write2Byte(process_handle, BASE_ADDR + 0x890 + YunTianHe * 0xb14, (UInt16)ALL_USER[YunTianHe].hp.max);
            if (Y_Q_S.Checked)
                Write2Byte(process_handle, BASE_ADDR + 0x894 + YunTianHe * 0xb14, (UInt16)ALL_USER[YunTianHe].rage.max);
            if (Y_S_S.Checked)
                Write2Byte(process_handle, BASE_ADDR + 0x898 + YunTianHe * 0xb14, (UInt16)ALL_USER[YunTianHe].mp.max);

            if (H_J_S.Checked)
                Write2Byte(process_handle, BASE_ADDR + 0x890 + HanLingSha * 0xb14, (UInt16)ALL_USER[HanLingSha].hp.max);
            if (H_Q_S.Checked)
                Write2Byte(process_handle, BASE_ADDR + 0x894 + HanLingSha * 0xb14, (UInt16)ALL_USER[HanLingSha].rage.max);
            if (H_S_S.Checked)
                Write2Byte(process_handle, BASE_ADDR + 0x898 + HanLingSha * 0xb14, (UInt16)ALL_USER[HanLingSha].mp.max);

            if (L_J_S.Checked)
                Write2Byte(process_handle, BASE_ADDR + 0x890 + LiuMengLi * 0xb14, (UInt16)ALL_USER[LiuMengLi].hp.max);
            if (L_Q_S.Checked)
                Write2Byte(process_handle, BASE_ADDR + 0x894 + LiuMengLi * 0xb14, (UInt16)ALL_USER[LiuMengLi].rage.max);
            if (L_S_S.Checked)
                Write2Byte(process_handle, BASE_ADDR + 0x898 + LiuMengLi * 0xb14, (UInt16)ALL_USER[LiuMengLi].mp.max);

            if (M_J_S.Checked)
                Write2Byte(process_handle, BASE_ADDR + 0x890 + MuRongZiYing * 0xb14, (UInt16)ALL_USER[MuRongZiYing].hp.max);
            if (M_Q_S.Checked)
                Write2Byte(process_handle, BASE_ADDR + 0x894 + MuRongZiYing * 0xb14, (UInt16)ALL_USER[MuRongZiYing].rage.max);
            if (M_S_S.Checked)
                Write2Byte(process_handle, BASE_ADDR + 0x898 + MuRongZiYing * 0xb14, (UInt16)ALL_USER[MuRongZiYing].mp.max);

            if (Y_W_S.Checked)
                Write2Byte(process_handle, BASE_ADDR + 0x668 + YunTianHe * 0xb14, 60000);
            if (Y_F_S.Checked)
                Write2Byte(process_handle, BASE_ADDR + 0x66c + YunTianHe * 0xb14, 60000);
            if (Y_S2_S.Checked)
                Write2Byte(process_handle, BASE_ADDR + 0x670 + YunTianHe * 0xb14, 60000);
            if (Y_Y_S.Checked)
                Write2Byte(process_handle, BASE_ADDR + 0x674 + YunTianHe * 0xb14, 60000);
            if (Y_L_S.Checked)
                Write2Byte(process_handle, BASE_ADDR + 0x678 + YunTianHe * 0xb14, 60000);

            if (H_W_S.Checked)
                Write2Byte(process_handle, BASE_ADDR + 0x668 + HanLingSha * 0xb14, 60000);
            if (H_F_S.Checked)
                Write2Byte(process_handle, BASE_ADDR + 0x66c + HanLingSha * 0xb14, 60000);
            if (H_S2_S.Checked)
                Write2Byte(process_handle, BASE_ADDR + 0x670 + HanLingSha * 0xb14, 60000);
            if (H_Y_S.Checked)
                Write2Byte(process_handle, BASE_ADDR + 0x674 + HanLingSha * 0xb14, 60000);
            if (H_L_S.Checked)
                Write2Byte(process_handle, BASE_ADDR + 0x678 + HanLingSha * 0xb14, 60000);


            if (L_W_S.Checked)
                Write2Byte(process_handle, BASE_ADDR + 0x668 + LiuMengLi * 0xb14, 60000);
            if (L_F2_S.Checked)
                Write2Byte(process_handle, BASE_ADDR + 0x66c + LiuMengLi * 0xb14, 60000);
            if (L_S2_S.Checked)
                Write2Byte(process_handle, BASE_ADDR + 0x670 + LiuMengLi * 0xb14, 60000);
            if (L_Y_S.Checked)
                Write2Byte(process_handle, BASE_ADDR + 0x674 + LiuMengLi * 0xb14, 60000);
            if (L_L_S.Checked)
                Write2Byte(process_handle, BASE_ADDR + 0x678 + LiuMengLi * 0xb14, 60000);

            if (M_W_S.Checked)
                Write2Byte(process_handle, BASE_ADDR + 0x668 + MuRongZiYing * 0xb14, 60000);
            if (M_F2_S.Checked)
                Write2Byte(process_handle, BASE_ADDR + 0x66c + MuRongZiYing * 0xb14, 60000);
            if (M_S2_S.Checked)
                Write2Byte(process_handle, BASE_ADDR + 0x670 + MuRongZiYing * 0xb14, 60000);
            if (M_Y_S.Checked)
                Write2Byte(process_handle, BASE_ADDR + 0x674 + MuRongZiYing * 0xb14, 60000);
            if (M_L_S.Checked)
                Write2Byte(process_handle, BASE_ADDR + 0x678 + MuRongZiYing * 0xb14, 60000);


            if (is_fight)//战斗时的数据锁定
            {
                Int32 out_data = 0;
                Int32 FIGHT_ADDR = 0;

                Int32[] BASE_ADDR_FIGHT = new Int32[3];//人物基址  从作往右0 1 2

                //获取人物战斗地址
                if (Read4Byte(process_handle, 0x8F3128 + OffsetValue, out out_data) == false)
                    return;
                if (Read4Byte(process_handle, out_data + 0x34, out out_data) == false)
                    return;
                FIGHT_ADDR = out_data;


                //获取左侧人物地址
                if (Read4Byte(process_handle, FIGHT_ADDR, out BASE_ADDR_FIGHT[0]) == false)
                    return;

                if (Read4Byte(process_handle, FIGHT_ADDR + 4, out BASE_ADDR_FIGHT[1]) == false)
                    return;

                if (Read4Byte(process_handle, FIGHT_ADDR + 8, out BASE_ADDR_FIGHT[2]) == false)
                    return;

                Int32[] f_data = new Int32[3];
                Read4Byte(process_handle, BASE_ADDR_FIGHT[0] + FightOffsetValue, out f_data[0]);
                Read4Byte(process_handle, BASE_ADDR_FIGHT[1] + FightOffsetValue, out f_data[1]);
                Read4Byte(process_handle, BASE_ADDR_FIGHT[2] + FightOffsetValue, out f_data[2]);

                //如果左边在队伍中
                if (f_data[0] == 0x846008 && FIGHT_USER[0].inTeam == true)
                {
                    if (L_F_J_S.Checked == true)
                    {
                        if (Write2Byte(process_handle, BASE_ADDR_FIGHT[0] + 0x23C, (UInt16)FIGHT_USER[0].hp.max) == false)
                            return;
                    }

                    if (L_F_Q_S.Checked == true)
                    {
                        if (Write2Byte(process_handle, BASE_ADDR_FIGHT[0] + 0x240, (UInt16)FIGHT_USER[0].rage.max) == false)
                            return;
                    }

                    if (L_F_S_S.Checked == true)
                    {
                        if (Write2Byte(process_handle, BASE_ADDR_FIGHT[0] + 0x244, (UInt16)FIGHT_USER[0].mp.max) == false)
                            return;
                    }
                }

                if (f_data[1] == 0x846008 && FIGHT_USER[1].inTeam == true)
                {
                    if (M_F_J_S.Checked == true)
                    {
                        if (Write2Byte(process_handle, BASE_ADDR_FIGHT[1] + 0x23C, (UInt16)FIGHT_USER[1].hp.max) == false)
                            return;
                    }

                    if (M_F_Q_S.Checked == true)
                    {
                        if (Write2Byte(process_handle, BASE_ADDR_FIGHT[1] + 0x240, (UInt16)FIGHT_USER[1].rage.max) == false)
                            return;
                    }

                    if (M_F_S_S.Checked == true)
                    {
                        if (Write2Byte(process_handle, BASE_ADDR_FIGHT[1] + 0x244, (UInt16)FIGHT_USER[1].mp.max) == false)
                            return;
                    }
                }

                if (f_data[0] == 0x846008 && FIGHT_USER[0].inTeam == true)
                {
                    if (R_F_J_S.Checked == true)
                    {
                        if (Write2Byte(process_handle, BASE_ADDR_FIGHT[2] + 0x23C, (UInt16)FIGHT_USER[2].hp.max) == false)
                            return;
                    }

                    if (R_F_Q_S.Checked == true)
                    {
                        if (Write2Byte(process_handle, BASE_ADDR_FIGHT[2] + 0x240, (UInt16)FIGHT_USER[2].rage.max) == false)
                            return;
                    }

                    if (R_F_S_S.Checked == true)
                    {
                        if (Write2Byte(process_handle, BASE_ADDR_FIGHT[2] + 0x244, (UInt16)FIGHT_USER[2].mp.max) == false)
                            return;
                    }
                }
            }
        }
        //获得HP MP RAGE(战斗时)
        private void Get_HMR_Fight()
        {

            Int32 out_data = 0;
            Int32 FIGHT_ADDR = 0;

            Int32[] BASE_ADDR =  new Int32[3];//人物基址  从作往右0 1 2

            //获取人物战斗地址
            if (Read4Byte(process_handle, 0x8F3128 + OffsetValue, out out_data) == false)
                return;
            if (Read4Byte(process_handle, out_data + 0x34, out out_data) == false)
                return;
            FIGHT_ADDR = out_data;


            //获取左侧人物地址
            if (Read4Byte(process_handle, FIGHT_ADDR, out out_data) == false)
                return;
            BASE_ADDR[0] = out_data;

            if (Read4Byte(process_handle, FIGHT_ADDR + 4, out out_data) == false)
                return;
            BASE_ADDR[1] = out_data;

            if (Read4Byte(process_handle, FIGHT_ADDR + 8, out out_data) == false)
                return;
            BASE_ADDR[2] = out_data;

            Int32[] f_data = new Int32[3];
            Read4Byte(process_handle, BASE_ADDR[0] + FightOffsetValue, out f_data[0]);
            Read4Byte(process_handle, BASE_ADDR[1] + FightOffsetValue, out f_data[1]);
            Read4Byte(process_handle, BASE_ADDR[2] + FightOffsetValue, out f_data[2]);
            UInt16[] hp_now = new UInt16[3];//从作往右0 1 2
            UInt16[] hp_max = new UInt16[3];
            UInt16[] mp_now = new UInt16[3];
            UInt16[] mp_max = new UInt16[3];
            UInt16[] rage = new UInt16[3];
            
            for (int i = 0; i < 3; i++)//获取场上人物的数据
            {
                if (f_data[i] != 0x846008)
                {
                    FIGHT_USER[i].inTeam = false;
                    continue;
                }
                if (Read2Byte(process_handle, BASE_ADDR[i] + 0x158, out hp_max[i]) == false)
                    return;
                if (Read2Byte(process_handle, BASE_ADDR[i] + 0x23C, out hp_now[i]) == false)
                    return;

                if (Read2Byte(process_handle, BASE_ADDR[i] + 0x160, out mp_max[i]) == false)
                    return;
                if (Read2Byte(process_handle, BASE_ADDR[i] + 0x244, out mp_now[i]) == false)
                    return;

                if (Read2Byte(process_handle, BASE_ADDR[i] + 0x240, out rage[i]) == false)
                    return;
                FIGHT_USER[i].hp.max = hp_max[i];
                FIGHT_USER[i].hp.now = hp_now[i];

                FIGHT_USER[i].mp.max = mp_max[i];
                FIGHT_USER[i].mp.now = mp_now[i];

                FIGHT_USER[i].rage.max = 100;
                FIGHT_USER[i].rage.now = rage[i];
                FIGHT_USER[i].inTeam = true;
            }
        }
       
        private void button1_Click(object sender, EventArgs e)
        {
            run_fast.Checked = false;
            if ((int)process_handle != 0) W_API.CloseHandle(process_handle);
            //查找窗口
            window_hwnd = W_API.FindWindow(null, "PAL4-Application");
            if ((int)window_hwnd == 0)
            {
                MessageBox.Show("找不到窗口");
                return;
            }
            //获取PID
            W_API.GetWindowThreadProcessId(window_hwnd, out process_id);
            if(process_id == 0)
            {
                MessageBox.Show("查找进程ID失败");
                return;
            }
            //打开进程
            process_handle = W_API.OpenProcess(0x1F0FFF,0,(uint)process_id);
            if ((int)process_handle == 0)
            {
                MessageBox.Show("打开进程失败");
                return;
            }
            //Get_HP(process_handle);
            //MessageBox.Show("载入完成");
            Text = "仙剑4内存修改器 - PAL4.exe - " + process_id.ToString();
            timer1.Enabled = true;
            button1.Enabled = false;
            button1.Text = "加载完成";
        }
        //设置不遇敌
        private void set_no_boss()
        {
            Int32 b_addr = 0;
            if (Read4Byte(process_handle, 0x8E11FC + OffsetValue, out b_addr) == false) return;
            if (Write2Byte(process_handle, b_addr + 0x2E0, 0x01) == false) return;//设置不遇敌
            if (Write4Byte(process_handle, b_addr + 0x2E4, 0x040A00000) == false) return;//设置不遇敌时间
        }
        //迷宫点数无限
        private void set_flag()
        {
            Int32 b_addr = 0;
            if (Read4Byte(process_handle, 0x8F30E8 + OffsetValue, out b_addr) == false) return;
            if (Write2Byte(process_handle, b_addr + 0x34, 0x00) == false) return;//设置已放置点数为0
        }
        private void timer1_Tick(object sender, EventArgs e)
        {
            if(!g_m.Focused)
                g_m.Text = Get_Money().ToString();
            Get_Fight_Status();//获得战斗状态
            Get_HMR();
            Set_HMR();
            Show_Data();
            if (no_boss.Checked == true)
                set_no_boss();
            if (flag_infinite.Checked == true)
                set_flag();
            if(is_fight)
                Get_HMR_Fight();
            else
            {
                FIGHT_USER[0].inTeam = false;
                FIGHT_USER[1].inTeam = false;
                FIGHT_USER[2].inTeam = false;
            }
            if (h_no_boss.Checked)
                Set_HMODE();
            Set_Speed();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            Set_Money(int.Parse(g_m.Text));
        }

        private void Form1_FormClosed(object sender, FormClosedEventArgs e)
        {
            if ((int)process_handle != 0) W_API.CloseHandle(process_handle);
        }

        private void Set_Speed()
        {
            if (run_fast.Checked)
            {
                Int32 b_addr = 0;
                if (Read4Byte(process_handle, 0x8E11FC + OffsetValue, out b_addr) == false) return;
                if (Write4Byte(process_handle, b_addr + 0x60, 0x44600000) == false) return;//设置速度
            }
            else//点了取消 要恢复以前的速度
            {
                Int32 b_addr = 0;
                if (Read4Byte(process_handle, 0x8E11FC + OffsetValue, out b_addr) == false) return;
                Int32 run_mode = 0;//移动模式0：走 1：慢跑 2：快跑
                if (Read4Byte(process_handle, b_addr + 0x84, out run_mode) == false) return;//读出移动模式
                switch(run_mode)
                {
                    case 0:
                        if (Write4Byte(process_handle, b_addr + 0x60, 0x4287EB85) == false) return;//设置速度
                        break;
                    case 1:
                        if (Write4Byte(process_handle, b_addr + 0x60, 0x4329E666) == false) return;//设置速度
                        break;
                    case 2:
                        if (Write4Byte(process_handle, b_addr + 0x60, 0x437ED999) == false) return;//设置速度
                        break;
                }
            }
        }

        //设置高级不遇敌
        private void Set_HMODE()
        {
            Int32 BASE_ADDR = 0;
            if (Read4Byte(process_handle, 0x8E1428 + OffsetValue, out BASE_ADDR) == false)
                return;
            Write2Byte(process_handle, BASE_ADDR + 0xB08 + YunTianHe * 0xb14, 0x00);
            Write2Byte(process_handle, BASE_ADDR + 0xB08 + HanLingSha * 0xb14, 0x00);
            Write2Byte(process_handle, BASE_ADDR + 0xB08 + LiuMengLi * 0xb14, 0x00);
            Write2Byte(process_handle, BASE_ADDR + 0xB08 + MuRongZiYing * 0xb14, 0x00);
        }

        private void Y_I_T_CheckedChanged(object sender, EventArgs e)
        {
            UInt16 indt = 0;
            if (Y_I_T.Checked == false)
                indt = 0;
            else
                indt = 1;
            Int32 base_addr;
            if (Read4Byte(process_handle, 0x8E1428 + OffsetValue, out base_addr) == false)
                return;

            Write2Byte(process_handle,base_addr + 0xB08 + YunTianHe * 0xb14,indt);
        }

        private void H_I_T_CheckedChanged(object sender, EventArgs e)
        {
            UInt16 indt = 0;
            if (H_I_T.Checked == false)
                indt = 0;
            else
                indt = 1;
            Int32 base_addr;
            if (Read4Byte(process_handle, 0x8E1428 + OffsetValue, out base_addr) == false)
                return;

            Write2Byte(process_handle, base_addr + 0xB08 + HanLingSha * 0xb14, indt);
        }

        private void L_I_T_CheckedChanged(object sender, EventArgs e)
        {
            UInt16 indt = 0;
            if (L_I_T.Checked == false)
                indt = 0;
            else
                indt = 1;
            Int32 base_addr;
            if (Read4Byte(process_handle, 0x8E1428 + OffsetValue, out base_addr) == false)
                return;

            Write2Byte(process_handle, base_addr + 0xB08 + LiuMengLi * 0xb14, indt);
        }

        private void M_I_T_CheckedChanged(object sender, EventArgs e)
        {
            UInt16 indt = 0;
            if (M_I_T.Checked == false)
                indt = 0;
            else
                indt = 1;
            Int32 base_addr;
            if (Read4Byte(process_handle, 0x8E1428 + OffsetValue, out base_addr) == false)
                return;

            Write2Byte(process_handle, base_addr + 0xB08 + MuRongZiYing * 0xb14, indt);
        }

        private void nFightInFight() {
            if (fight_auto_lock.Checked) {
                L_F_J_S.Checked = true;
                L_F_Q_S.Checked = true;
                L_F_S_S.Checked = true;

                M_F_J_S.Checked = true;
                M_F_Q_S.Checked = true;
                M_F_S_S.Checked = true;

                R_F_J_S.Checked = true;
                R_F_Q_S.Checked = true;
                R_F_S_S.Checked = true;
            }
        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e) {
            if(comboBox1.SelectedIndex == 1) {
                OffsetValue = 0x3E30;
                FightOffsetValue = 0x2CC;
            }
            else {
                OffsetValue = 0;
                FightOffsetValue = 0;
            }
        }

        private void linkLabel1_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e) {
            System.Diagnostics.Process.Start("https://mfweb.top/");
        }

        private void linkLabel2_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e) {
            System.Diagnostics.Process.Start("https://github.com/Mfweb/PAL4-Edit");
        }

        private void fight_auto_lock_CheckedChanged(object sender, EventArgs e) {
            if(fight_auto_lock.Checked) {
                L_F_J_S.Checked = true;
                L_F_Q_S.Checked = true;
                L_F_S_S.Checked = true;

                M_F_J_S.Checked = true;
                M_F_Q_S.Checked = true;
                M_F_S_S.Checked = true;

                R_F_J_S.Checked = true;
                R_F_Q_S.Checked = true;
                R_F_S_S.Checked = true;
            }
            else {
                L_F_J_S.Checked = false;
                L_F_Q_S.Checked = false;
                L_F_S_S.Checked = false;

                M_F_J_S.Checked = false;
                M_F_Q_S.Checked = false;
                M_F_S_S.Checked = false;

                R_F_J_S.Checked = false;
                R_F_Q_S.Checked = false;
                R_F_S_S.Checked = false;
            }
        }

        private void button3_Click(object sender, EventArgs e) {
            Y_J_S.Checked = true;
            Y_Q_S.Checked = true;
            Y_S_S.Checked = true;

            Y_W_S.Checked = true;
            Y_F_S.Checked = true;
            Y_S2_S.Checked = true;
            Y_Y_S.Checked = true;
            Y_L_S.Checked = true;
        }

        private void button4_Click(object sender, EventArgs e) {
            H_J_S.Checked = true;
            H_Q_S.Checked = true;
            H_S_S.Checked = true;

            H_W_S.Checked = true;
            H_F_S.Checked = true;
            H_S2_S.Checked = true;
            H_Y_S.Checked = true;
            H_L_S.Checked = true;
        }

        private void button5_Click(object sender, EventArgs e) {
            L_J_S.Checked = true;
            L_Q_S.Checked = true;
            L_S_S.Checked = true;

            L_W_S.Checked = true;
            L_F2_S.Checked = true;
            L_S2_S.Checked = true;
            L_Y_S.Checked = true;
            L_L_S.Checked = true;
        }

        private void button6_Click(object sender, EventArgs e) {
            M_J_S.Checked = true;
            M_Q_S.Checked = true;
            M_S_S.Checked = true;

            M_W_S.Checked = true;
            M_F2_S.Checked = true;
            M_S2_S.Checked = true;
            M_Y_S.Checked = true;
            M_L_S.Checked = true;
        }
    }
}
