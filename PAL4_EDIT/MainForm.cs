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

namespace PAL4_EDIT {
    public partial class MainForm : Form {
        W_API memoryManager = new W_API();


        IntPtr window_hwnd = new IntPtr(0);
        Int32 process_id = 0;
        IntPtr process_handle = new IntPtr(0);

        bool isFighting = false;//是否正在战斗

        int OffsetValue = 0x3E30;
        int FightOffsetValue = 0x2CC;

        public struct Characters {
            public const int YunTianHe = 0;
            public const int HanLingSha = 1;
            public const int LiuMengLi = 2;
            public const int MuRongZiYing = 3;
        }

        public class baseAddress {
            public const int fightStatus = 0x8F3190; //战斗状态
            public const int money = 0x8EB064;       //金钱
            public const int character = 0x8E1428;   //角色基址
            public const int fightCharacter = 0x8F3128;//战斗时角色基址
            public const int characterStatus = 0x8E11FC;//角色状态
            public const int mapStatus = 0x8F30E8;//迷宫地图状态
        };

        /// <summary>
        /// 非战斗时角色数据显示控件
        /// </summary>
        public struct USER_OBJECT_NFight {
            public Label JingText;
            public Label QiText;
            public Label ShenText;

            public ProgressBar JingProgress;
            public ProgressBar QiProgress;
            public ProgressBar ShenProgress;

            public Label WuText;
            public Label FangText;
            public Label SuText;
            public Label YunText;
            public Label LingText;

            public GroupBox TopText;

            public CheckBox InTeam;
        };

        /// <summary>
        /// 非战斗锁定CheckBox
        /// </summary>
        public struct USER_OBJECT_LOCK_NFight {
            public CheckBox Jing;
            public CheckBox Qi;
            public CheckBox Shen;
            public CheckBox Wu;
            public CheckBox Fang;
            public CheckBox Su;
            public CheckBox Yun;
            public CheckBox Ling;
        };

        /// <summary>
        /// 角色在非战斗时的数据
        /// </summary>
        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        public struct UserUnFightData {
            [MarshalAsAttribute(UnmanagedType.ByValArray, SizeConst = 409, ArraySubType = UnmanagedType.I4)]
            public Int32[] Reserved1;

            public UInt32 wu;//668
            public UInt32 fang;
            public UInt32 su;
            public UInt32 yun;
            public UInt32 ling;//678

            [MarshalAsAttribute(UnmanagedType.ByValArray, SizeConst = 74, ArraySubType = UnmanagedType.I4)]
            public Int32[] Reserved2;
            public UInt32 pos;//7a0

            [MarshalAsAttribute(UnmanagedType.ByValArray, SizeConst = 2, ArraySubType = UnmanagedType.I4)]
            public Int32[] Reserved3;

            public UInt32 hp_max;//7ac

            public UInt32 Reserved4;

            public UInt32 mp_max;//7b4
            public UInt32 wu_final;
            public UInt32 fang_final;
            public UInt32 su_final;
            public UInt32 yun_final;
            public UInt32 ling_final;//7c8

            [MarshalAsAttribute(UnmanagedType.ByValArray, SizeConst = 46, ArraySubType = UnmanagedType.I4)]
            public Int32[] Reserved5;

            public UInt32 level; //884

            [MarshalAsAttribute(UnmanagedType.ByValArray, SizeConst = 2, ArraySubType = UnmanagedType.I4)]
            public Int32[] Reserved6;

            public UInt32 hp_now;//890
            public UInt32 rage_now;
            public UInt32 mp_now;//898

            [MarshalAsAttribute(UnmanagedType.ByValArray, SizeConst = 155, ArraySubType = UnmanagedType.I4)]
            public Int32[] Reserved7;

            public UInt32 inTeam;//b08
        };
        /// <summary>
        /// 非战斗时用户显示数组
        /// </summary>
        public struct USER_DISPLAY_DATA {
            public string name;
            public USER_OBJECT_NFight obj;
            public USER_OBJECT_LOCK_NFight lockopt;
        }



        /// <summary>
        /// 战斗锁定CheckBox
        /// </summary>
        public struct USER_OBJECT_LOCK_Fight {
            public CheckBox Jing;
            public CheckBox Qi;
            public CheckBox Shen;
        };

        /// <summary>
        /// 战斗时角色数据显示控件
        /// </summary>
        public struct USER_OBJECT_Fight {
            public Label JingText;
            public Label QiText;
            public Label ShenText;

            public ProgressBar JingProgress;
            public ProgressBar QiProgress;
            public ProgressBar ShenProgress;

            public GroupBox Group;
        };
        /// <summary>
        /// 战斗时数据
        /// </summary>
        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        public struct FIGHT_USER_DATA {
            [MarshalAsAttribute(UnmanagedType.ByValArray, SizeConst = 86, ArraySubType = UnmanagedType.I4)]
            public Int32[] Reserved1;

            public UInt32 hp_max;//158

            [MarshalAsAttribute(UnmanagedType.ByValArray, SizeConst = 1, ArraySubType = UnmanagedType.I4)]
            public Int32[] Reserved2;

            public UInt32 mp_max;//160

            [MarshalAsAttribute(UnmanagedType.ByValArray, SizeConst = 54, ArraySubType = UnmanagedType.I4)]
            public Int32[] Reserved3;

            public UInt32 hp_now;//23c
            public UInt32 rage_now;//240
            public UInt32 mp_now;//244

            public bool inTeam;//这个位置是否有人            
        };

        /// <summary>
        /// 战斗时用户显示数组
        /// </summary>
        public struct FIGHT_USER_DATA_DISPLAY {
            public USER_OBJECT_Fight obj;
            public USER_OBJECT_LOCK_Fight lockopt;
        }

        UserUnFightData[] ALL_USER = new UserUnFightData[4];
        USER_DISPLAY_DATA[] ALL_USER_DISPLAY = new USER_DISPLAY_DATA[4];
        FIGHT_USER_DATA[] FIGHT_USER = new FIGHT_USER_DATA[3];
        FIGHT_USER_DATA_DISPLAY[] FIGHT_USER_DISPLAY = new FIGHT_USER_DATA_DISPLAY[3];
        public MainForm() {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e) {
            comboBox1.SelectedIndex = 1;

            ALL_USER_DISPLAY[0].name = "云天河";
            ALL_USER_DISPLAY[0].obj.InTeam = Y_I_T;
            ALL_USER_DISPLAY[0].obj.TopText = Y_G_T;
            ALL_USER_DISPLAY[0].obj.JingText = Y_J_L;
            ALL_USER_DISPLAY[0].obj.QiText = Y_Q_L;
            ALL_USER_DISPLAY[0].obj.ShenText = Y_S_L;
            ALL_USER_DISPLAY[0].obj.WuText = D_Y_W;
            ALL_USER_DISPLAY[0].obj.FangText = D_Y_F;
            ALL_USER_DISPLAY[0].obj.SuText = D_Y_S;
            ALL_USER_DISPLAY[0].obj.YunText = D_Y_Y;
            ALL_USER_DISPLAY[0].obj.LingText = D_Y_L;
            ALL_USER_DISPLAY[0].obj.JingProgress = Y_J;
            ALL_USER_DISPLAY[0].obj.QiProgress = Y_Q;
            ALL_USER_DISPLAY[0].obj.ShenProgress = Y_S;
            ALL_USER_DISPLAY[0].lockopt.Jing = Y_J_S;
            ALL_USER_DISPLAY[0].lockopt.Qi = Y_Q_S;
            ALL_USER_DISPLAY[0].lockopt.Shen = Y_S_S;
            ALL_USER_DISPLAY[0].lockopt.Wu = Y_W_S;
            ALL_USER_DISPLAY[0].lockopt.Fang = Y_F_S;
            ALL_USER_DISPLAY[0].lockopt.Su = Y_S2_S;
            ALL_USER_DISPLAY[0].lockopt.Yun = Y_Y_S;
            ALL_USER_DISPLAY[0].lockopt.Ling = Y_L_S;

            ALL_USER_DISPLAY[1].name = "韩菱纱";
            ALL_USER_DISPLAY[1].obj.InTeam = H_I_T;
            ALL_USER_DISPLAY[1].obj.TopText = H_G_T;
            ALL_USER_DISPLAY[1].obj.JingText = H_J_L;
            ALL_USER_DISPLAY[1].obj.QiText = H_Q_L;
            ALL_USER_DISPLAY[1].obj.ShenText = H_S_L;
            ALL_USER_DISPLAY[1].obj.WuText = D_H_W;
            ALL_USER_DISPLAY[1].obj.FangText = D_H_F;
            ALL_USER_DISPLAY[1].obj.SuText = D_H_S;
            ALL_USER_DISPLAY[1].obj.YunText = D_H_Y;
            ALL_USER_DISPLAY[1].obj.LingText = D_H_L;
            ALL_USER_DISPLAY[1].obj.JingProgress = H_J;
            ALL_USER_DISPLAY[1].obj.QiProgress = H_Q;
            ALL_USER_DISPLAY[1].obj.ShenProgress = H_S;
            ALL_USER_DISPLAY[1].lockopt.Jing = H_J_S;
            ALL_USER_DISPLAY[1].lockopt.Qi = H_Q_S;
            ALL_USER_DISPLAY[1].lockopt.Shen = H_S_S;
            ALL_USER_DISPLAY[1].lockopt.Wu = H_W_S;
            ALL_USER_DISPLAY[1].lockopt.Fang = H_F_S;
            ALL_USER_DISPLAY[1].lockopt.Su = H_S2_S;
            ALL_USER_DISPLAY[1].lockopt.Yun = H_Y_S;
            ALL_USER_DISPLAY[1].lockopt.Ling = H_L_S;

            ALL_USER_DISPLAY[2].name = "柳梦璃";
            ALL_USER_DISPLAY[2].obj.InTeam = L_I_T;
            ALL_USER_DISPLAY[2].obj.TopText = L_G_T;
            ALL_USER_DISPLAY[2].obj.JingText = L_J_L;
            ALL_USER_DISPLAY[2].obj.QiText = L_Q_L;
            ALL_USER_DISPLAY[2].obj.ShenText = L_S_L;
            ALL_USER_DISPLAY[2].obj.WuText = D_L_W;
            ALL_USER_DISPLAY[2].obj.FangText = D_L_F;
            ALL_USER_DISPLAY[2].obj.SuText = D_L_S;
            ALL_USER_DISPLAY[2].obj.YunText = D_L_Y;
            ALL_USER_DISPLAY[2].obj.LingText = D_L_L;
            ALL_USER_DISPLAY[2].obj.JingProgress = L_J;
            ALL_USER_DISPLAY[2].obj.QiProgress = L_Q;
            ALL_USER_DISPLAY[2].obj.ShenProgress = L_S;
            ALL_USER_DISPLAY[2].lockopt.Jing = L_J_S;
            ALL_USER_DISPLAY[2].lockopt.Qi = L_Q_S;
            ALL_USER_DISPLAY[2].lockopt.Shen = L_S_S;
            ALL_USER_DISPLAY[2].lockopt.Wu = L_W_S;
            ALL_USER_DISPLAY[2].lockopt.Fang = L_F2_S;
            ALL_USER_DISPLAY[2].lockopt.Su = L_S2_S;
            ALL_USER_DISPLAY[2].lockopt.Yun = L_Y_S;
            ALL_USER_DISPLAY[2].lockopt.Ling = L_L_S;


            ALL_USER_DISPLAY[3].name = "慕容紫英";
            ALL_USER_DISPLAY[3].obj.InTeam = M_I_T;
            ALL_USER_DISPLAY[3].obj.TopText = M_G_T;
            ALL_USER_DISPLAY[3].obj.JingText = M_J_L;
            ALL_USER_DISPLAY[3].obj.QiText = M_Q_L;
            ALL_USER_DISPLAY[3].obj.ShenText = M_S_L;
            ALL_USER_DISPLAY[3].obj.WuText = D_M_W;
            ALL_USER_DISPLAY[3].obj.FangText = D_M_F;
            ALL_USER_DISPLAY[3].obj.SuText = D_M_S;
            ALL_USER_DISPLAY[3].obj.YunText = D_M_Y;
            ALL_USER_DISPLAY[3].obj.LingText = D_M_L;
            ALL_USER_DISPLAY[3].obj.JingProgress = M_J;
            ALL_USER_DISPLAY[3].obj.QiProgress = M_Q;
            ALL_USER_DISPLAY[3].obj.ShenProgress = M_S;
            ALL_USER_DISPLAY[3].lockopt.Jing = M_J_S;
            ALL_USER_DISPLAY[3].lockopt.Qi = M_Q_S;
            ALL_USER_DISPLAY[3].lockopt.Shen = M_S_S;
            ALL_USER_DISPLAY[3].lockopt.Wu = M_W_S;
            ALL_USER_DISPLAY[3].lockopt.Fang = M_F2_S;
            ALL_USER_DISPLAY[3].lockopt.Su = M_S2_S;
            ALL_USER_DISPLAY[3].lockopt.Yun = M_Y_S;
            ALL_USER_DISPLAY[3].lockopt.Ling = M_L_S;




            FIGHT_USER_DISPLAY[0].obj.Group = L_U_G;
            FIGHT_USER_DISPLAY[0].obj.JingText = L_F_J_L;
            FIGHT_USER_DISPLAY[0].obj.QiText = L_F_Q_L;
            FIGHT_USER_DISPLAY[0].obj.ShenText = L_F_S_L;
            FIGHT_USER_DISPLAY[0].obj.JingProgress = L_F_J;
            FIGHT_USER_DISPLAY[0].obj.QiProgress = L_F_Q;
            FIGHT_USER_DISPLAY[0].obj.ShenProgress = L_F_S;
            FIGHT_USER_DISPLAY[0].lockopt.Jing = L_F_J_S;
            FIGHT_USER_DISPLAY[0].lockopt.Qi = L_F_Q_S;
            FIGHT_USER_DISPLAY[0].lockopt.Shen = L_F_S_S;

            FIGHT_USER_DISPLAY[1].obj.Group = M_U_G;
            FIGHT_USER_DISPLAY[1].obj.JingText = M_F_J_L;
            FIGHT_USER_DISPLAY[1].obj.QiText = M_F_Q_L;
            FIGHT_USER_DISPLAY[1].obj.ShenText = M_F_S_L;
            FIGHT_USER_DISPLAY[1].obj.JingProgress = M_F_J;
            FIGHT_USER_DISPLAY[1].obj.QiProgress = M_F_Q;
            FIGHT_USER_DISPLAY[1].obj.ShenProgress = M_F_S;
            FIGHT_USER_DISPLAY[1].lockopt.Jing = M_F_J_S;
            FIGHT_USER_DISPLAY[1].lockopt.Qi = M_F_Q_S;
            FIGHT_USER_DISPLAY[1].lockopt.Shen = M_F_S_S;

            FIGHT_USER_DISPLAY[2].obj.Group = R_U_G;
            FIGHT_USER_DISPLAY[2].obj.JingText = R_F_J_L;
            FIGHT_USER_DISPLAY[2].obj.QiText = R_F_Q_L;
            FIGHT_USER_DISPLAY[2].obj.ShenText = R_F_S_L;
            FIGHT_USER_DISPLAY[2].obj.JingProgress = R_F_J;
            FIGHT_USER_DISPLAY[2].obj.QiProgress = R_F_Q;
            FIGHT_USER_DISPLAY[2].obj.ShenProgress = R_F_S;
            FIGHT_USER_DISPLAY[2].lockopt.Jing = R_F_J_S;
            FIGHT_USER_DISPLAY[2].lockopt.Qi = R_F_Q_S;
            FIGHT_USER_DISPLAY[2].lockopt.Shen = R_F_S_S;
        }
    
        //获得战斗状态
        private void Get_Fight_Status() {
            Int32 out_data = 0;
            if (memoryManager.Read4Byte(process_handle, baseAddress.fightStatus + OffsetValue, out out_data) == false) {
                isFighting = false;
                Text = "仙剑4内存修改器 - PAL4.exe - " + process_id.ToString() + " - " + "战斗状态错误";
                return;
            }

            if (isFighting == false && out_data != 0) {
                nFightInFight();
            }

            if (out_data == 0) isFighting = false;
            else isFighting = true;
            Text = "仙剑4内存修改器 - PAL4.exe - " + process_id.ToString() + (isFighting == true ? " - 战斗中" : "");
            //Text = "仙剑4内存修改器 - PAL4.exe - " + process_id.ToString() + " - " + out_data.ToString("X");
        }
        //获取金钱
        private Int32 Get_Money() {
            Int32 out_data = 0;
            if (memoryManager.Read4Byte(process_handle, baseAddress.money + OffsetValue, out out_data) == false) {
                return 0;
            }

            if (memoryManager.Read4Byte(process_handle, out_data + 0x134, out out_data) == false) {
                return 0;
            }
            //MessageBox.Show("Money:" + out_data.ToString());
            return out_data;
        }
        //设置金钱
        private bool Set_Money(Int32 Money) {
            Int32 out_data = 0;

            if (memoryManager.Read4Byte(process_handle, baseAddress.money + OffsetValue, out out_data) == false) {
                MessageBox.Show("读取失败");
                return false;
            }

            if (memoryManager.Write4Byte(process_handle, out_data + 0x134, Money) == false) {
                MessageBox.Show("写入失败" + W_API.GetLastError().ToString());
                return false;
            }
            MessageBox.Show("写入成功");
            return true;
        }

        private void Show_Data() {
            try {
                for (int id = 0; id < 4; id++) {
                    string displayPos = 
                    ALL_USER_DISPLAY[id].obj.JingText.Text = ALL_USER[id].hp_now.ToString() + "/" + ALL_USER[id].hp_max.ToString();
                    ALL_USER_DISPLAY[id].obj.QiText.Text = ALL_USER[id].rage_now.ToString() + "/100";
                    ALL_USER_DISPLAY[id].obj.ShenText.Text = ALL_USER[id].mp_now.ToString() + "/" + ALL_USER[id].mp_max.ToString();

                    ALL_USER_DISPLAY[id].obj.JingProgress.Maximum = (int)ALL_USER[id].hp_max;
                    ALL_USER_DISPLAY[id].obj.JingProgress.Value = (int)ALL_USER[id].hp_now;

                    ALL_USER_DISPLAY[id].obj.QiProgress.Maximum = 100;
                    ALL_USER_DISPLAY[id].obj.QiProgress.Value = (int)ALL_USER[id].rage_now;

                    ALL_USER_DISPLAY[id].obj.ShenProgress.Maximum = (int)ALL_USER[id].mp_max;
                    ALL_USER_DISPLAY[id].obj.ShenProgress.Value = (int)ALL_USER[id].mp_now;

                    ALL_USER_DISPLAY[id].obj.WuText.Text = "武：" + ALL_USER[id].wu.ToString() + "(" + ALL_USER[id].wu_final.ToString() + ")";
                    ALL_USER_DISPLAY[id].obj.FangText.Text = "防：" + ALL_USER[id].fang.ToString() + "(" + ALL_USER[id].fang_final.ToString() + ")";
                    ALL_USER_DISPLAY[id].obj.SuText.Text = "速：" + ALL_USER[id].su.ToString() + "(" + ALL_USER[id].su_final.ToString() + ")";
                    ALL_USER_DISPLAY[id].obj.YunText.Text = "运：" + ALL_USER[id].yun.ToString() + "(" + ALL_USER[id].yun_final.ToString() + ")";
                    ALL_USER_DISPLAY[id].obj.LingText.Text = "灵：" + ALL_USER[id].ling.ToString() + "(" + ALL_USER[id].ling_final.ToString() + ")";


                    ALL_USER_DISPLAY[id].obj.TopText.Text = ALL_USER_DISPLAY[id].name + " - " + ALL_USER[id].level.ToString() + " - " + ALL_USER[id].pos.ToString();

                    ALL_USER_DISPLAY[id].obj.InTeam.Checked = ALL_USER[id].inTeam == 1? true: false;
                }

                for (int i = 0; i < 3; i++) {
                    FIGHT_USER_DISPLAY[i].obj.Group.Visible = FIGHT_USER[i].inTeam;
                    if (FIGHT_USER[i].inTeam == false) {
                        continue;
                    }
                    FIGHT_USER_DISPLAY[i].obj.JingText.Text = FIGHT_USER[i].hp_now.ToString() + "/" + FIGHT_USER[i].hp_max.ToString();
                    FIGHT_USER_DISPLAY[i].obj.QiText.Text = FIGHT_USER[i].rage_now.ToString() + "/100";
                    FIGHT_USER_DISPLAY[i].obj.ShenText.Text = FIGHT_USER[i].mp_now.ToString() + "/" + FIGHT_USER[i].mp_max.ToString();
                    
                    FIGHT_USER_DISPLAY[i].obj.JingProgress.Maximum = (int)FIGHT_USER[i].hp_max;
                    FIGHT_USER_DISPLAY[i].obj.JingProgress.Value = (int)FIGHT_USER[i].hp_now;

                    FIGHT_USER_DISPLAY[i].obj.QiProgress.Maximum = 100;
                    FIGHT_USER_DISPLAY[i].obj.QiProgress.Value = (int)FIGHT_USER[i].rage_now;

                    FIGHT_USER_DISPLAY[i].obj.ShenProgress.Maximum = (int)FIGHT_USER[i].mp_max;
                    FIGHT_USER_DISPLAY[i].obj.ShenProgress.Value = (int)FIGHT_USER[i].mp_now;
            
                }
            }
            catch (Exception) {
            }

        }

        //获得HP MP RAGE(非战斗时)
        unsafe private void Get_HMR() {

            Int32 out_data = 0;
            Int32 BASE_ADDR = 0;
            if (memoryManager.Read4Byte(process_handle, baseAddress.character + OffsetValue, out out_data) == false) {
                return;
            }
            BASE_ADDR = out_data;
            for (int id = 0; id < 4; id++) {
                IntPtr readP = Marshal.AllocHGlobal(Marshal.SizeOf(ALL_USER[id].GetType()));
                //Marshal.StructureToPtr(ALL_USER[id], readP, true);

                memoryManager.ReadBytes(process_handle, BASE_ADDR + id * 0xb14,2828, (byte *)readP);
                ALL_USER[id] = (UserUnFightData)Marshal.PtrToStructure(readP, ALL_USER[id].GetType());
                Marshal.FreeHGlobal(readP);
            }
            //MessageBox.Show("HP:" + hp.ToString());
        }
        //锁定数据处理
        private void Set_HMR() {
            Int32 BASE_ADDR = 0;
            if (memoryManager.Read4Byte(process_handle, baseAddress.character + OffsetValue, out BASE_ADDR) == false)
                return;

            for(int i = 0; i < 4; i++) {
                if(ALL_USER_DISPLAY[i].lockopt.Jing.Checked) {
                    memoryManager.Write2Byte(process_handle, BASE_ADDR + 0x890 + i * 0xb14, (UInt16)ALL_USER[i].hp_max);
                }
                if (ALL_USER_DISPLAY[i].lockopt.Qi.Checked) {
                    memoryManager.Write2Byte(process_handle, BASE_ADDR + 0x894 + i * 0xb14, 100);
                }
                if (ALL_USER_DISPLAY[i].lockopt.Shen.Checked) {
                    memoryManager.Write2Byte(process_handle, BASE_ADDR + 0x898 + i * 0xb14, (UInt16)ALL_USER[i].mp_max);
                }

                if (ALL_USER_DISPLAY[i].lockopt.Wu.Checked) {
                    memoryManager.Write2Byte(process_handle, BASE_ADDR + 0x668 + i * 0xb14, 60000);
                }
                if (ALL_USER_DISPLAY[i].lockopt.Fang.Checked) {
                    memoryManager.Write2Byte(process_handle, BASE_ADDR + 0x66c + i * 0xb14, 60000);
                }
                if (ALL_USER_DISPLAY[i].lockopt.Su.Checked) {
                    memoryManager.Write2Byte(process_handle, BASE_ADDR + 0x670 + i * 0xb14, 60000);
                }
                if (ALL_USER_DISPLAY[i].lockopt.Yun.Checked) {
                    memoryManager.Write2Byte(process_handle, BASE_ADDR + 0x674 + i * 0xb14, 60000);
                }
                if (ALL_USER_DISPLAY[i].lockopt.Ling.Checked) {
                    memoryManager.Write2Byte(process_handle, BASE_ADDR + 0x678 + i * 0xb14, 60000);
                }
            }

            if (isFighting)//战斗时的数据锁定
            {
                Int32 out_data = 0;
                Int32 FIGHT_ADDR = 0;

                Int32[] BASE_ADDR_FIGHT = new Int32[3];//人物基址  从作往右0 1 2

                //获取人物战斗地址
                if (memoryManager.Read4Byte(process_handle, baseAddress.fightCharacter + OffsetValue, out out_data) == false)
                    return;
                if (memoryManager.Read4Byte(process_handle, out_data + 0x34, out out_data) == false)
                    return;
                FIGHT_ADDR = out_data;


                //获取左侧人物地址
                if (memoryManager.Read4Byte(process_handle, FIGHT_ADDR, out BASE_ADDR_FIGHT[0]) == false)
                    return;

                if (memoryManager.Read4Byte(process_handle, FIGHT_ADDR + 4, out BASE_ADDR_FIGHT[1]) == false)
                    return;

                if (memoryManager.Read4Byte(process_handle, FIGHT_ADDR + 8, out BASE_ADDR_FIGHT[2]) == false)
                    return;

                Int32[] f_data = new Int32[3];
                memoryManager.Read4Byte(process_handle, BASE_ADDR_FIGHT[0] + FightOffsetValue, out f_data[0]);
                memoryManager.Read4Byte(process_handle, BASE_ADDR_FIGHT[1] + FightOffsetValue, out f_data[1]);
                memoryManager.Read4Byte(process_handle, BASE_ADDR_FIGHT[2] + FightOffsetValue, out f_data[2]);

                for (int i = 0; i < 3; i++) {
                    if (f_data[i] == 0x846008 && FIGHT_USER[i].inTeam == true) {
                        if(FIGHT_USER_DISPLAY[i].lockopt.Jing.Checked) {
                            memoryManager.Write2Byte(process_handle, BASE_ADDR_FIGHT[i] + 0x23C, (UInt16)FIGHT_USER[i].hp_max);
                        }
                        if (FIGHT_USER_DISPLAY[i].lockopt.Qi.Checked) {
                            memoryManager.Write2Byte(process_handle, BASE_ADDR_FIGHT[i] + 0x240, 100);
                        }
                        if (FIGHT_USER_DISPLAY[i].lockopt.Shen.Checked) {
                            memoryManager.Write2Byte(process_handle, BASE_ADDR_FIGHT[i] + 0x244, (UInt16)FIGHT_USER[i].mp_max);
                        }
                    }
                }
            }
        }
        //获得HP MP RAGE(战斗时)
        unsafe private void Get_HMR_Fight() {
            Int32 FIGHT_ADDR = 0;

            Int32[] BASE_ADDR = new Int32[3];//人物基址  从作往右0 1 2

            //获取人物战斗地址
            if (memoryManager.Read4Byte(process_handle, baseAddress.fightCharacter + OffsetValue, out FIGHT_ADDR) == false)
                return;
            if (memoryManager.Read4Byte(process_handle, FIGHT_ADDR + 0x34, out FIGHT_ADDR) == false)
                return;


            //获取左侧人物地址
            if (memoryManager.Read4Byte(process_handle, FIGHT_ADDR, out BASE_ADDR[0]) == false)
                return;

            if (memoryManager.Read4Byte(process_handle, FIGHT_ADDR + 4, out BASE_ADDR[1]) == false)
                return;

            if (memoryManager.Read4Byte(process_handle, FIGHT_ADDR + 8, out BASE_ADDR[2]) == false)
                return;


            for (int i = 0; i < 3; i++)//获取场上人物的数据
            {
                Int32 inTeamCheck = 0;
                memoryManager.Read4Byte(process_handle, BASE_ADDR[i] + FightOffsetValue, out inTeamCheck);
                if (inTeamCheck != 0x846008) {
                    FIGHT_USER[i].inTeam = false;
                    continue;
                }

                IntPtr readP = Marshal.AllocHGlobal(Marshal.SizeOf(FIGHT_USER[i].GetType()));
                //Marshal.StructureToPtr(ALL_USER[id], readP, true);

                memoryManager.ReadBytes(process_handle, BASE_ADDR[i], 584, (byte*)readP);
                FIGHT_USER[i] = (FIGHT_USER_DATA)Marshal.PtrToStructure(readP, FIGHT_USER[i].GetType());
                Marshal.FreeHGlobal(readP);
                FIGHT_USER[i].inTeam = true;
            }
        }

        private void button1_Click(object sender, EventArgs e) {
            run_fast.Checked = false;
            if ((int)process_handle != 0) W_API.CloseHandle(process_handle);
            //查找窗口
            window_hwnd = W_API.FindWindow(null, "PAL4-Application");
            if ((int)window_hwnd == 0) {
                MessageBox.Show("找不到窗口");
                return;
            }
            //获取PID
            W_API.GetWindowThreadProcessId(window_hwnd, out process_id);
            if (process_id == 0) {
                MessageBox.Show("查找进程ID失败");
                return;
            }
            //打开进程
            process_handle = W_API.OpenProcess(0x1F0FFF, 0, (uint)process_id);
            if ((int)process_handle == 0) {
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
        private void set_no_boss() {
            Int32 b_addr = 0;
            if (memoryManager.Read4Byte(process_handle, baseAddress.characterStatus + OffsetValue, out b_addr) == false) return;
            if (memoryManager.Write2Byte(process_handle, b_addr + 0x2E0, 0x01) == false) return;//设置不遇敌
            if (memoryManager.Write4Byte(process_handle, b_addr + 0x2E4, 0x040A00000) == false) return;//设置不遇敌时间
        }
        //迷宫点数无限
        private void set_flag() {
            Int32 b_addr = 0;
            if (memoryManager.Read4Byte(process_handle, baseAddress.mapStatus + OffsetValue, out b_addr) == false) return;
            if (memoryManager.Write2Byte(process_handle, b_addr + 0x34, 0x00) == false) return;//设置已放置点数为0
        }
        private void timer1_Tick(object sender, EventArgs e) {
            if (!g_m.Focused)
                g_m.Text = Get_Money().ToString();
            Get_Fight_Status();//获得战斗状态
            Get_HMR();
            Set_HMR();
            
            if (no_boss.Checked == true)
                set_no_boss();
            if (flag_infinite.Checked == true)
                set_flag();
            if (isFighting)
                Get_HMR_Fight();
            else {
                FIGHT_USER[0].inTeam = false;
                FIGHT_USER[1].inTeam = false;
                FIGHT_USER[2].inTeam = false;
            }
            if (h_no_boss.Checked)
                Set_HMODE();
            Set_Speed();

            Show_Data();
        }

        private void button2_Click(object sender, EventArgs e) {
            Set_Money(int.Parse(g_m.Text));
        }

        private void Form1_FormClosed(object sender, FormClosedEventArgs e) {
            if ((int)process_handle != 0) W_API.CloseHandle(process_handle);
        }

        private void Set_Speed() {
            if (run_fast.Checked) {
                Int32 b_addr = 0;
                if (memoryManager.Read4Byte(process_handle, baseAddress.characterStatus + OffsetValue, out b_addr) == false) return;
                if (memoryManager.Write4Byte(process_handle, b_addr + 0x60, 0x44600000) == false) return;//设置速度
            }
            else//点了取消 要恢复以前的速度
            {
                Int32 b_addr = 0;
                if (memoryManager.Read4Byte(process_handle, baseAddress.characterStatus + OffsetValue, out b_addr) == false) return;
                Int32 run_mode = 0;//移动模式0：走 1：慢跑 2：快跑
                if (memoryManager.Read4Byte(process_handle, b_addr + 0x84, out run_mode) == false) return;//读出移动模式
                switch (run_mode) {
                    case 0:
                        if (memoryManager.Write4Byte(process_handle, b_addr + 0x60, 0x4287EB85) == false) return;//设置速度
                        break;
                    case 1:
                        if (memoryManager.Write4Byte(process_handle, b_addr + 0x60, 0x4329E666) == false) return;//设置速度
                        break;
                    case 2:
                        if (memoryManager.Write4Byte(process_handle, b_addr + 0x60, 0x437ED999) == false) return;//设置速度
                        break;
                }
            }
        }

        //设置高级不遇敌
        private void Set_HMODE() {
            Int32 BASE_ADDR = 0;
            if (memoryManager.Read4Byte(process_handle, baseAddress.character + OffsetValue, out BASE_ADDR) == false)
                return;

            for(int i = 0; i < 4; i++) {
                memoryManager.Write2Byte(process_handle, BASE_ADDR + 0xB08 + i * 0xb14, 0x00);
            }
        }

        private void Y_I_T_CheckedChanged(object sender, EventArgs e) {
            UInt16 indt = 0;
            if (Y_I_T.Checked == false)
                indt = 0;
            else
                indt = 1;
            Int32 base_addr;
            if (memoryManager.Read4Byte(process_handle, baseAddress.character + OffsetValue, out base_addr) == false)
                return;

            memoryManager.Write2Byte(process_handle, base_addr + 0xB08 + Characters.YunTianHe * 0xb14, indt);
        }

        private void H_I_T_CheckedChanged(object sender, EventArgs e) {
            UInt16 indt = 0;
            if (H_I_T.Checked == false)
                indt = 0;
            else
                indt = 1;
            Int32 base_addr;
            if (memoryManager.Read4Byte(process_handle, baseAddress.character + OffsetValue, out base_addr) == false)
                return;

            memoryManager.Write2Byte(process_handle, base_addr + 0xB08 + Characters.HanLingSha * 0xb14, indt);
        }

        private void L_I_T_CheckedChanged(object sender, EventArgs e) {
            UInt16 indt = 0;
            if (L_I_T.Checked == false)
                indt = 0;
            else
                indt = 1;
            Int32 base_addr;
            if (memoryManager.Read4Byte(process_handle, baseAddress.character + OffsetValue, out base_addr) == false)
                return;

            memoryManager.Write2Byte(process_handle, base_addr + 0xB08 + Characters.LiuMengLi * 0xb14, indt);
        }

        private void M_I_T_CheckedChanged(object sender, EventArgs e) {
            UInt16 indt = 0;
            if (M_I_T.Checked == false)
                indt = 0;
            else
                indt = 1;
            Int32 base_addr;
            if (memoryManager.Read4Byte(process_handle, baseAddress.character + OffsetValue, out base_addr) == false)
                return;

            memoryManager.Write2Byte(process_handle, base_addr + 0xB08 + Characters.MuRongZiYing * 0xb14, indt);
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
            if (comboBox1.SelectedIndex == 1) {
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

        private void lockUnFight(int id) {
            ALL_USER_DISPLAY[id].lockopt.Jing.Checked = true;
            ALL_USER_DISPLAY[id].lockopt.Qi.Checked = true;
            ALL_USER_DISPLAY[id].lockopt.Shen.Checked = true;

            ALL_USER_DISPLAY[id].lockopt.Wu.Checked = true;
            ALL_USER_DISPLAY[id].lockopt.Fang.Checked = true;
            ALL_USER_DISPLAY[id].lockopt.Su.Checked = true;
            ALL_USER_DISPLAY[id].lockopt.Yun.Checked = true;
            ALL_USER_DISPLAY[id].lockopt.Ling.Checked = true;
        }

        private void button3_Click(object sender, EventArgs e) {
            lockUnFight(Characters.YunTianHe);
        }

        private void button4_Click(object sender, EventArgs e) {
            lockUnFight(Characters.HanLingSha);
        }

        private void button5_Click(object sender, EventArgs e) {
            lockUnFight(Characters.LiuMengLi);
        }

        private void button6_Click(object sender, EventArgs e) {
            lockUnFight(Characters.MuRongZiYing);
        }
    }
}
