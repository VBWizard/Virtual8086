using Word = System.UInt16;
using DWord = System.UInt32;
using QWord = System.UInt64;
using System.Collections.Generic;

namespace VirtualProcessor.Devices
{


   
    public struct mScanCode
    {
        public QWord Make;
        public QWord Break;
        public mScanCode(QWord iMake, QWord iBreak)
        {
            Make = iMake;
            Break = iBreak;
        }
    }

    public partial class cKbdDevice : cDevice
    {


        public Dictionary<string, int> ScanCodeDict = new Dictionary<string, int>() { {"",0x1},
                        {"1",0x2},
                        {"2",0x3},
                        {"3",0x4},
                        {"4",0x5},
                        {"5",0x6},
                        {"6",0x7},
                        {"7",0x8},
                        {"8",0x9},
                        {"9",0x0A},
                        {"0",0x0B},
                        {"-",0x0C},
                        {"=",0x0D},
                        {"BACKSPACE",0x0E},
                        {"TAB",0x0F},
                        {"Q",0x10},
                        {"W",0x11},
                        {"E",0x12},
                        {"R",0x13},
                        {"T",0x14},
                        {"Y",0x15},
                        {"U",0x16},
                        {"I",0x17},
                        {"O",0x18},
                        {"P",0x19},
                        {"[",0x1A},
                        {"]",0x1B},
                        {"ENTER",0x1C},
                        {"A",0x1E},
                        {"S",0x1F},
                        {"D",0x20},
                        {"F",0x21},
                        {"G",0x22},
                        {"H",0x23},
                        {"J",0x24},
                        {"K",0x25},
                        {"L",0x26},
                        {";",0x27},
                        {"‘",0x28},
                        {"`",0x29},
                        {@"\",0x2B},
                        {"Z",0x2C},
                        {"X",0x2D},
                        {"C",0x2E},
                        {"V",0x2F},
                        {"B",0x30},
                        {"N",0x31},
                        {"M",0x32},
                        {",",0x33},
                        {".",0x34},
                        {"/",0x35},
                        {"SPACEBAR",0x39},
                        {"F1",0x3B},
                        {"F2",0x3C},
                        {"F3",0x3D},
                        {"F4",0x3E},
                        {"F5",0x3F},
                        {"F6",0x40},
                        {"F7",0x41},
                        {"F8",0x42},
                        {"F9",0x43},
                        {"F10",0x44},
                        {"F11",0x45},
                        {"F12",0x46},
                        {"HOME",0x47},
                        {"UPARROW",0x48},
                        {"PAGEUP",0x49},
                        {"SUBTRACT",0x4A},
                        {"LEFTARROW",0x4B},
                        {"CLEAR",0x4C},
                        {"RIGHTARROW",0x4D},
                        {"ADD",0x4E},
                        {"END",0x4F},
                        {"DOWNARROW",0x50},
                        {"PAGEDOWN",0x51},
                        {"INSERT",0x52},
                        {"DELETE",0x53},
                        {"!",0x2},
                        {"@",0x3},
                        {"#",0x4},
                        {"$",0x5},
                        {"%",0x6},
                        {"^",0x7},
                        {"&",0x8},
                        {"*",0x9},
                        {"(",0x0A},
                        {")",0x0B},
                        {"_",0x0C},
                        {"+",0x0D},
                        {"{",0x1A},
                        {"}",0x1B},
                        {":",0x27},
                        {"“",0x28},
                        {"~",0x29},
                        {"|",0x2B},
                        {"<",0x33},
                        {">",0x34},
                        {"?",0x35},
                        {"\"",0x28},
                        {"LSHFT",0x12},
                        {"RSHFT",0x59}};

        
        #region ScanCode array Declaration
        mScanCode[,] scancodes = new mScanCode[KEYBOARD_NUMBER_OF_KEYS, 3]
        {
        { // BX_KEY_CTRL_L ( ibm 58)
           new mScanCode( 0x1D , 0x9D ),
           new mScanCode(  0x14, 0xF014 ),
           new mScanCode(  0x11, 0xF011),
         },

         { // BX_KEY_SHIFT_L ( ibm 44)
           new mScanCode(  0x2A, 0xAA),
           new mScanCode(  0x12, 0xF012),
           new mScanCode(  0x12, 0xF012),
         },

         { // BX_KEY_F1 ( ibm 112 )
           new mScanCode(  0x3B, 0xBB),
           new mScanCode(  0x05, 0xF005),
           new mScanCode(  0x07, 0xF007),
         },

         { // BX_KEY_F2 ( ibm 113 ) 
           new mScanCode(  0x3C, 0xBC),
           new mScanCode(  0x06, 0xF006),
           new mScanCode(  0x0F, 0xF00F),
         },

         { // BX_KEY_F3 ( ibm 114 ) 
           new mScanCode(  0x3D, 0xBD),
           new mScanCode(  0x04, 0xF004),
           new mScanCode(  0x17, 0xF017),
         },

         { // BX_KEY_F4 ( ibm 115 ) 
           new mScanCode(  0x3E, 0xBE),
           new mScanCode(  0x0C, 0xF00C),
           new mScanCode(  0x1F, 0xF01F),
         },

         { // BX_KEY_F5 ( ibm 116 ) 
           new mScanCode(  0x3F, 0xBF),
           new mScanCode(  0x03, 0xF003),
           new mScanCode(  0x27, 0xF027),
         },

         { // BX_KEY_F6 ( ibm 117 ) 
           new mScanCode(  0x40, 0xC0),
           new mScanCode(  0x0B, 0xF00B),
           new mScanCode(  0x2F, 0xF02F),
         },

         { // BX_KEY_F7 ( ibm 118 ) 
           new mScanCode(  0x41, 0xC1),
           new mScanCode(  0x83, 0xF083),
           new mScanCode(  0x37, 0xF037),
        },

         { // BX_KEY_F8 ( ibm 119 ) 
           new mScanCode(  0x42, 0xC2),
           new mScanCode(  0x0A, 0xF00A),
           new mScanCode(  0x3F, 0xF03F),
         },

         { // BX_KEY_F9 ( ibm 120 ) 
           new mScanCode(  0x43, 0xC3),
           new mScanCode(  0x01, 0xF001),
           new mScanCode(  0x47, 0xF047),
         },

         { // BX_KEY_F10 ( ibm 121 ) 
           new mScanCode(  0x44, 0xC4),
           new mScanCode(  0x09, 0xF009),
           new mScanCode(  0x4F, 0xF04F),
         },

         { // BX_KEY_F11 ( ibm 122 ) 
           new mScanCode(  0x57, 0xD7),
           new mScanCode(  0x78, 0xF078),
           new mScanCode(  0x56, 0xF056),
         },

         { // BX_KEY_F12 ( ibm 123 ) 
           new mScanCode(  0x58, 0xD8),
           new mScanCode(  0x07, 0xF007),
           new mScanCode(  0x5E, 0xF05E),
         },

         { // BX_KEY_CTRL_R ( ibm 64 ) 
           new mScanCode(  0xE01D, 0xE09D),
           new mScanCode(  0xE014, 0xE0F014),
           new mScanCode(  0x58, 0xF58),
         },

         { // BX_KEY_SHIFT_R ( ibm 57 ) 
           new mScanCode(  0x36, 0xB6),
           new mScanCode(  0x59, 0xF059),
           new mScanCode(  0x59, 0xF059),
         },

         { // BX_KEY_CAPS_LOCK ( ibm 30 ) 
           new mScanCode(  0x3A, 0xBA),
           new mScanCode(  0x58, 0xF058),
           new mScanCode(  0x14, 0xF014),
         },

         { // BX_KEY_NUM_LOCK ( ibm 90 ) 
           new mScanCode(  0x45, 0xC5),
           new mScanCode(  0x77, 0xF077),
           new mScanCode(  0x76, 0xF076),
         },

         { // BX_KEY_ALT_L ( ibm 60 ) 
           new mScanCode(  0x38, 0xB8),
           new mScanCode(  0x11, 0xF011),
           new mScanCode(  0x19, 0xF019),
         },

         { // BX_KEY_ALT_R ( ibm 62 ) 
           new mScanCode(  0xE038, 0xE0B8),
           new mScanCode(  0xE011, 0xE0F011),
           new mScanCode(  0x39, 0xF039),
         },

         { // BX_KEY_A ( ibm 31 ) 
           new mScanCode(  0x1E, 0x9E),
           new mScanCode(  0x1C, 0xF01C),
           new mScanCode(  0x1C, 0xF01C),
         },

         { // BX_KEY_B ( ibm 50 ) 
           new mScanCode(  0x30, 0xB0),
           new mScanCode(  0x32, 0xF032),
           new mScanCode(  0x32, 0xF032),
         },

         { // BX_KEY_C ( ibm 48 ) 
           new mScanCode(  0x2E, 0xAE),
           new mScanCode(  0x21, 0xF021),
           new mScanCode(  0x21, 0xF021),
         },

         { // BX_KEY_D ( ibm 33 ) 
           new mScanCode(  0x20, 0xA0),
           new mScanCode(  0x23, 0xF023),
           new mScanCode(  0x23, 0xF023),
         },

         { // BX_KEY_E ( ibm 19 ) 
           new mScanCode(  0x12, 0x92),
           new mScanCode(  0x24, 0xF024),
           new mScanCode(  0x24, 0xF024),
         },

         { // BX_KEY_F ( ibm 34 ) 
           new mScanCode(  0x21, 0xA1),
           new mScanCode(  0x2B, 0xF02B),
           new mScanCode(  0x2B, 0xF02B),
         },

         { // BX_KEY_G ( ibm 35 ) 
           new mScanCode(  0x22, 0xA2),
           new mScanCode(  0x34, 0xF034),
           new mScanCode(  0x34, 0xF034),
         },

         { // BX_KEY_H ( ibm 36 ) 
           new mScanCode(  0x23, 0xA3),
           new mScanCode(  0x33, 0xF033),
           new mScanCode(  0x33, 0xF033),
         },

         { // BX_KEY_I ( ibm 24 ) 
           new mScanCode(  0x17, 0x97),
           new mScanCode(  0x43, 0xF043),
           new mScanCode(  0x43, 0xF043),
         },

         { // BX_KEY_J ( ibm 37 ) 
           new mScanCode(  0x24, 0xA4),
           new mScanCode(  0x3B, 0xF03B),
           new mScanCode(  0x3B, 0xF03B),
         },

         { // BX_KEY_K ( ibm 38 ) 
           new mScanCode(  0x25, 0xA5),
           new mScanCode(  0x42, 0xF042),
           new mScanCode(  0x42, 0xF042),
         },

         { // BX_KEY_L ( ibm 39 ) 
           new mScanCode(  0x26, 0xA6),
           new mScanCode(  0x4B, 0xF04B),
           new mScanCode(  0x4B, 0xF04B),
         },

         { // BX_KEY_M ( ibm 52 ) 
           new mScanCode(  0x32, 0xB2),
           new mScanCode(  0x3A, 0xF03A),
           new mScanCode(  0x3A, 0xF03A),
         },

         { // BX_KEY_N ( ibm 51 ) 
           new mScanCode(  0x31, 0xB1),
           new mScanCode(  0x31, 0xF031),
           new mScanCode(  0x31, 0xF031),
         },

         { // BX_KEY_O ( ibm 25 ) 
           new mScanCode(  0x18, 0x98),
           new mScanCode(  0x44, 0xF044),
           new mScanCode(  0x44, 0xF044),
         },

         { // BX_KEY_P ( ibm 26 ) 
           new mScanCode(  0x19, 0x99),
           new mScanCode(  0x4D, 0xF04D),
           new mScanCode(  0x4D, 0xF04D),
         },

         { // BX_KEY_Q ( ibm 17 ) 
           new mScanCode(  0x10, 0x90),
           new mScanCode(  0x15, 0xF015),
           new mScanCode(  0x15, 0xF015),
         },

         { // BX_KEY_R ( ibm 20 ) 
           new mScanCode(  0x13, 0x93),
           new mScanCode(  0x2D, 0xF02D),
           new mScanCode(  0x2D, 0xF02D),
         },

         { // BX_KEY_S ( ibm 32 ) 
           new mScanCode(  0x1F, 0x9F),
           new mScanCode(  0x1B, 0xF01B),
           new mScanCode(  0x1B, 0xF01B),
         },

         { // BX_KEY_T ( ibm 21 ) 
           new mScanCode(  0x14, 0x94),
           new mScanCode(  0x2C, 0xF02C),
           new mScanCode(  0x2C, 0xF02C),
         },

         { // BX_KEY_U ( ibm 23 ) 
           new mScanCode(  0x16, 0x96),
           new mScanCode(  0x3C, 0xF03C),
           new mScanCode(  0x3C, 0xF03C),
         },

         { // BX_KEY_V ( ibm 49 ) 
           new mScanCode(  0x2F, 0xAF),
           new mScanCode(  0x2A, 0xF02A),
           new mScanCode(  0x2A, 0xF02A),
         },

         { // BX_KEY_W ( ibm 18 ) 
           new mScanCode(  0x11, 0x91),
           new mScanCode(  0x1D, 0xF01D),
           new mScanCode(  0x1D, 0xF01D),
         },

         { // BX_KEY_X ( ibm 47 ) 
           new mScanCode(  0x2D, 0xAD),
           new mScanCode(  0x22, 0xF022),
           new mScanCode(  0x22, 0xF022),
         },

         { // BX_KEY_Y ( ibm 22 ) 
           new mScanCode(  0x15, 0x95),
           new mScanCode(  0x35, 0xF035),
           new mScanCode(  0x35, 0xF035),
         },

         { // BX_KEY_Z ( ibm 46 ) 
           new mScanCode(  0x2C, 0xAC),
           new mScanCode(  0x1A, 0xF01A),
           new mScanCode(  0x1A, 0xF01A),
         },

         { // BX_KEY_0 ( ibm 11 ) 
           new mScanCode(  0x0B, 0x8B),
           new mScanCode(  0x45, 0xF045),
           new mScanCode(  0x45, 0xF045),
         },

         { // BX_KEY_1 ( ibm 2 ) 
           new mScanCode(  0x02, 0x82),
           new mScanCode(  0x16, 0xF016),
           new mScanCode(  0x16, 0xF016),
         },

         { // BX_KEY_2 ( ibm 3 ) 
           new mScanCode(  0x03, 0x83),
           new mScanCode(  0x1E, 0xF01E),
           new mScanCode(  0x1E, 0xF01E),
         },

         { // BX_KEY_3 ( ibm 4 ) 
           new mScanCode(  0x04, 0x84),
           new mScanCode(  0x26, 0xF026),
           new mScanCode(  0x26, 0xF026),
         },

         { // BX_KEY_4 ( ibm 5 ) 
           new mScanCode(  0x05, 0x85),
           new mScanCode(  0x25, 0xF025),
           new mScanCode(  0x25, 0xF025),
         },

         { // BX_KEY_5 ( ibm 6 ) 
           new mScanCode(  0x06, 0x86),
           new mScanCode(  0x2E, 0xF02E),
           new mScanCode(  0x2E, 0xF02E),
         },

         { // BX_KEY_6 ( ibm 7 ) 
           new mScanCode(  0x07, 0x87),
           new mScanCode(  0x36, 0xF036),
           new mScanCode(  0x36, 0xF036),
         },

         { // BX_KEY_7 ( ibm 8 ) 
           new mScanCode(  0x08, 0x88),
           new mScanCode(  0x3D, 0xF03D),
           new mScanCode(  0x3D, 0xF03D),
         },

         { // BX_KEY_8 ( ibm 9 ) 
           new mScanCode(  0x09, 0x89),
           new mScanCode(  0x3E, 0xF03E),
           new mScanCode(  0x3E, 0xF03E),
         },

         { // BX_KEY_9 ( ibm 10 ) 
           new mScanCode(  0x0A, 0x8A),
           new mScanCode(  0x46, 0xF046),
           new mScanCode(  0x46, 0xF046),
         },

         { // BX_KEY_ESC ( ibm 110 ) 
           new mScanCode(  0x01, 0x81),
           new mScanCode(  0x76, 0xF076),
           new mScanCode(  0x08, 0xF008),
         },

         { // BX_KEY_SPACE ( ibm 61 ) 
           new mScanCode(  0x39, 0xB9),
           new mScanCode(  0x29, 0xF029),
           new mScanCode(  0x29, 0xF029),
         },

         { // BX_KEY_SINGLE_QUOTE ( ibm 41 ) 
           new mScanCode(  0x28, 0xA8),
           new mScanCode(  0x52, 0xF052),
           new mScanCode(  0x52, 0xF052),
         },

         { // BX_KEY_COMMA ( ibm 53 ) 
           new mScanCode(  0x33, 0xB3),
           new mScanCode(  0x41, 0xF041),
           new mScanCode(  0x41, 0xF041),
         },

         { // BX_KEY_PERIOD ( ibm 54 ) 
           new mScanCode(  0x34, 0xB4),
           new mScanCode(  0x49, 0xF049),
           new mScanCode(  0x49, 0xF049),
         },

         { // BX_KEY_SLASH ( ibm 55 ) 
           new mScanCode(  0x35, 0xB5),
           new mScanCode(  0x4A, 0xF04A),
           new mScanCode(  0x4A, 0xF04A),
         },

         { // BX_KEY_SEMICOLON ( ibm 40 ) 
           new mScanCode(  0x27, 0xA7),
           new mScanCode(  0x4C, 0xF04C),
           new mScanCode(  0x4C, 0xF04C),
         },

         { // BX_KEY_EQUALS ( ibm 13 ) 
           new mScanCode(  0x0D, 0x8D),
           new mScanCode(  0x55, 0xF055),
           new mScanCode(  0x55, 0xF055),
         },

         { // BX_KEY_LEFT_BRACKET ( ibm 27 ) 
           new mScanCode(  0x1A, 0x9A),
           new mScanCode(  0x54, 0xF054),
           new mScanCode(  0x54, 0xF054),
         },

         { // BX_KEY_BACKSLASH ( ibm 42, 0x29)
           new mScanCode(  0x2B, 0xAB),
           new mScanCode(  0x5D, 0xF05D),
           new mScanCode(  0x53, 0xF053),
         },

         { // BX_KEY_RIGHT_BRACKET ( ibm 28 ) 
           new mScanCode(  0x1B, 0x9B),
           new mScanCode(  0x5B, 0xF05B),
           new mScanCode(  0x5B, 0xF05B),
         },

         { // BX_KEY_MINUS ( ibm 12 ) 
           new mScanCode(  0x0C, 0x8C),
           new mScanCode(  0x4E, 0xF04E),
           new mScanCode(  0x4E, 0xF04E),
         },

         { // BX_KEY_GRAVE ( ibm 1 ) 
           new mScanCode(  0x29, 0xA9),
           new mScanCode(  0x0E, 0xF00E),
           new mScanCode(  0x0E, 0xF00E),
         },

         { // BX_KEY_BACKSPACE ( ibm 15 ) 
           new mScanCode(  0x0E, 0x8E),
           new mScanCode(  0x66, 0xF066),
           new mScanCode(  0x66, 0xF066),
         },

         { // BX_KEY_ENTER ( ibm 43 ) 
           new mScanCode(  0x1C, 0x9C),
           new mScanCode(  0x5A, 0xF05A),
           new mScanCode(  0x5A, 0xF05A),
         },

         { // BX_KEY_TAB ( ibm 16 ) 
           new mScanCode(  0x0F, 0x8F),
           new mScanCode(  0x0D, 0xF00D),
           new mScanCode(  0x0D, 0xF00D),
         },

         { // BX_KEY_LEFT_BACKSLASH ( ibm 45 ) 
           new mScanCode(  0x56, 0xD6),
           new mScanCode(  0x61, 0xF061),
           new mScanCode(  0x13, 0xF013),
         },

         { // BX_KEY_PRINT ( ibm 124 ) 
           new mScanCode(  0xE037, 0xE0B7),
           new mScanCode(  0xE07C, 0xE0F07C),
           new mScanCode(  0x57, 0xF057),
         },

         { // BX_KEY_SCRL_LOCK ( ibm 125 ) 
           new mScanCode(  0x46, 0xC6),
           new mScanCode(  0x7E, 0xF07E),
           new mScanCode(  0x5F, 0xF05F),
         },

         { // BX_KEY_PAUSE ( ibm 126 ) 
           new mScanCode(  0xE11D45E19DC5, 0x0),
           new mScanCode(  0xE11477E1F014F077, 0x0),
           new mScanCode(  0x62, 0xF062),
         },

         { // BX_KEY_INSERT ( ibm 75 ) 
           new mScanCode(  0xE052, 0xE0D2),
           new mScanCode(  0xE070, 0xE0F070),
           new mScanCode(  0x67, 0xF067),
         },

         { // BX_KEY_DELETE ( ibm 76 ) 
           new mScanCode(  0xE053, 0xE0D3),
           new mScanCode(  0xE071, 0xE0F071),
           new mScanCode(  0x64, 0xF064),
         },

         { // BX_KEY_HOME ( ibm 80 ) 
           new mScanCode(  0xE047, 0xE0C7),
           new mScanCode(  0xE06C, 0xE0F06C),
           new mScanCode(  0x6E, 0xF06E),
         },

         { // BX_KEY_END ( ibm 81 ) 
           new mScanCode(  0xE04F, 0xE0CF),
           new mScanCode(  0xE069, 0xE0F069),
           new mScanCode(  0x65, 0xF065),
         },

         { // BX_KEY_PAGE_UP ( ibm 85 ) 
           new mScanCode(  0xE049, 0xE0C9),
           new mScanCode(  0xE07D, 0xE0F07D),
           new mScanCode(  0x6F, 0xF06F),
         },

         { // BX_KEY_PAGE_DOWN ( ibm 86 ) 
           new mScanCode(  0xE051, 0xE0D1),
           new mScanCode(  0xE07A, 0xE0F07A),
           new mScanCode(  0x6D, 0xF06D),
         },

         { // BX_KEY_KP_ADD ( ibm 106 ) 
           new mScanCode(  0x4E, 0xCE),
           new mScanCode(  0x79, 0xF079),
           new mScanCode(  0x7C, 0xF07C),
         },

         { // BX_KEY_KP_SUBTRACT ( ibm 105 ) 
           new mScanCode(  0x4A, 0xCA),
           new mScanCode(  0x7B, 0xF07B),
           new mScanCode(  0x84, 0xF084),
         },

         { // BX_KEY_KP_END ( ibm 93 ) 
           new mScanCode(  0x4F, 0xCF),
           new mScanCode(  0x69, 0xF069),
           new mScanCode(  0x69, 0xF069),
         },

         { // BX_KEY_KP_DOWN ( ibm 98 ) 
           new mScanCode(  0x50, 0xD0),
           new mScanCode(  0x72, 0xF072),
           new mScanCode(  0x72, 0xF072),
         },

         { // BX_KEY_KP_PAGE_DOWN ( ibm 103 ) 
           new mScanCode(  0x51, 0xD1),
           new mScanCode(  0x7A, 0xF07A),
           new mScanCode(  0x7A, 0xF07A),
         },

         { // BX_KEY_KP_LEFT ( ibm 92 ) 
           new mScanCode(  0x4B, 0xCB),
           new mScanCode(  0x6B, 0xF06B),
           new mScanCode(  0x6B, 0xF06B),
         },

         { // BX_KEY_KP_RIGHT ( ibm 102 ) 
           new mScanCode(  0x4D, 0xCD),
           new mScanCode(  0x74, 0xF074),
           new mScanCode(  0x74, 0xF074),
         },

         { // BX_KEY_KP_HOME ( ibm 91 ) 
           new mScanCode(  0x47, 0xC7),
           new mScanCode(  0x6C, 0xF06C),
           new mScanCode(  0x6C, 0xF06C),
         },

         { // BX_KEY_KP_UP ( ibm 96 ) 
           new mScanCode(  0x48, 0xC8),
           new mScanCode(  0x75, 0xF075),
           new mScanCode(  0x75, 0xF075),
         },

         { // BX_KEY_KP_PAGE_UP ( ibm 101 ) 
           new mScanCode(  0x49, 0xC9),
           new mScanCode(  0x7D, 0xF07D),
           new mScanCode(  0x7D, 0xF07D),
         },

         { // BX_KEY_KP_INSERT ( ibm 99 ) 
           new mScanCode(  0x52, 0xD2),
           new mScanCode(  0x70, 0xF070),
           new mScanCode(  0x70, 0xF070),
         },

         { // BX_KEY_KP_DELETE ( ibm 104 ) 
           new mScanCode(  0x53, 0xD3),
           new mScanCode(  0x71, 0xF071),
           new mScanCode(  0x71, 0xF071),
         },

         { // BX_KEY_KP_5 ( ibm 97 ) 
           new mScanCode(  0x4C, 0xCC),
           new mScanCode(  0x73, 0xF073),
           new mScanCode(  0x73, 0xF073),
         },

         { // BX_KEY_UP ( ibm 83 ) 
           new mScanCode(  0xE048, 0xE0C8),
           new mScanCode(  0xE075, 0xE0F075),
           new mScanCode(  0x63, 0xF063),
         },

         { // BX_KEY_DOWN ( ibm 84 ) 
           new mScanCode(  0xE050, 0xE0D0),
           new mScanCode(  0xE072, 0xE0F072),
           new mScanCode(  0x60, 0xF060),
         },

         { // BX_KEY_LEFT ( ibm 79 ) 
           new mScanCode(  0xE04B, 0xE0CB),
           new mScanCode(  0xE06B, 0xE0F06B),
           new mScanCode(  0x61, 0xF061),
         },

         { // BX_KEY_RIGHT ( ibm 89 ) 
           new mScanCode(  0xE04D, 0xE0CD),
           new mScanCode(  0xE074, 0xE0F074),
           new mScanCode(  0x6A, 0xF06A),
         },

         { // BX_KEY_KP_ENTER ( ibm 108 ) 
           new mScanCode(  0xE01C, 0xE09C),
           new mScanCode(  0xE05A, 0xE0F05A),
           new mScanCode(  0x79, 0xF079),
         },

         { // BX_KEY_KP_MULTIPLY ( ibm 100 ) 
           new mScanCode(  0x37, 0xB7),
           new mScanCode(  0x7C, 0xF07C),
           new mScanCode(  0x7E, 0xF07E),
         },

         { // BX_KEY_KP_DIVIDE ( ibm 95 ) 
           new mScanCode(  0xE035, 0xE0B5),
           new mScanCode(  0xE04A, 0xE0F04A),
           new mScanCode(  0x77, 0xF077),
         },

         { // BX_KEY_WIN_L 
           new mScanCode(  0xE05B, 0xE0DB),
           new mScanCode(  0xE01F, 0xE0F01F),
           new mScanCode(  0x8B, 0xF08B),
         },

         { // BX_KEY_WIN_R
           new mScanCode(  0xE05C, 0xE0DC),
           new mScanCode(  0xE027, 0xE0F027),
           new mScanCode(  0x8C, 0xF08C),
         },

         { // BX_KEY_MENU
           new mScanCode(  0xE05D, 0xE0DD),
           new mScanCode(  0xE02F, 0xE0F02F),
           new mScanCode(  0x8D, 0xF08D),
         },

         { // BX_KEY_ALT_SYSREQ
           new mScanCode(  0x54, 0xD4),
           new mScanCode(  0x84, 0xF084),
           new mScanCode(  0x57, 0xF057),
         },

         { // BX_KEY_CTRL_BREAK
           new mScanCode(  0xE046, 0xE0C6),
           new mScanCode(  0xE07E, 0xE0F07E),
           new mScanCode(  0x62, 0xF062),
         },

         { // BX_KEY_INT_BACK
           new mScanCode(  0xE06A, 0xE0EA),
           new mScanCode(  0xE038, 0xE0F038),
           new mScanCode(  0x38, 0xF038),
         },

         { // BX_KEY_INT_FORWARD
           new mScanCode(  0xE069, 0xE0E9),
           new mScanCode(  0xE030, 0xE0F030),
           new mScanCode(  0x30, 0xF030),
         },

         { // BX_KEY_INT_STOP
           new mScanCode(  0xE068, 0xE0E8),
           new mScanCode(  0xE028, 0xE0F028),
           new mScanCode(  0x28, 0xF028),
         },

         { // BX_KEY_INT_MAIL
           new mScanCode(  0xE06C, 0xE0EC),
           new mScanCode(  0xE048, 0xE0F048),
           new mScanCode(  0x48, 0xF048),
         },

         { // BX_KEY_INT_SEARCH
           new mScanCode(  0xE065, 0xE0E5),
           new mScanCode(  0xE010, 0xE0F010),
           new mScanCode(  0x10, 0xF010),
         },

         { // BX_KEY_INT_FAV
           new mScanCode(  0xE066, 0xE0E6),
           new mScanCode(  0xE018, 0xE0F018),
           new mScanCode(  0x18, 0xF018),
         },

         { // BX_KEY_INT_HOME
           new mScanCode(  0xE032, 0xE0B2),
           new mScanCode(  0xE03A, 0xE0F03A),
           new mScanCode(  0x97, 0xF097),
         },

         { // BX_KEY_POWER_MYCOMP
           new mScanCode(  0xE06B, 0xE0EB),
           new mScanCode(  0xE040, 0xE0F040),
           new mScanCode(  0x40, 0xF040),
         },

         { // BX_KEY_POWER_CALC
           new mScanCode(  0xE021, 0xE0A1),
           new mScanCode(  0xE02B, 0xE0F02B),
           new mScanCode(  0x99, 0xF099),
         },

         { // BX_KEY_POWER_SLEEP
           new mScanCode(  0xE05F, 0xE0DF),
           new mScanCode(  0xE03F, 0xE0F03F),
           new mScanCode(  0x7F, 0xF07F),
         },

         { // BX_KEY_POWER_POWER
           new mScanCode(  0xE05E, 0xE0DE),
           new mScanCode(  0xE037, 0xE0F037),
           new mScanCode( 0x0 , 0x0),
         },

         { // BX_KEY_POWER_WAKE
           new mScanCode(  0xE063, 0xE0E3),
           new mScanCode(  0xE05E, 0xE0F05E),
           new mScanCode( 0x0 , 0x0),
         },
    };
        #endregion

        byte[] translation8042 = new byte[256] {
          0xff,0x43,0x41,0x3f,0x3d,0x3b,0x3c,0x58,0x64,0x44,0x42,0x40,0x3e,0x0f,0x29,0x59,
          0x65,0x38,0x2a,0x70,0x1d,0x10,0x02,0x5a,0x66,0x71,0x2c,0x1f,0x1e,0x11,0x03,0x5b,
          0x67,0x2e,0x2d,0x20,0x12,0x05,0x04,0x5c,0x68,0x39,0x2f,0x21,0x14,0x13,0x06,0x5d,
          0x69,0x31,0x30,0x23,0x22,0x15,0x07,0x5e,0x6a,0x72,0x32,0x24,0x16,0x08,0x09,0x5f,
          0x6b,0x33,0x25,0x17,0x18,0x0b,0x0a,0x60,0x6c,0x34,0x35,0x26,0x27,0x19,0x0c,0x61,
          0x6d,0x73,0x28,0x74,0x1a,0x0d,0x62,0x6e,0x3a,0x36,0x1c,0x1b,0x75,0x2b,0x63,0x76,
          0x55,0x56,0x77,0x78,0x79,0x7a,0x0e,0x7b,0x7c,0x4f,0x7d,0x4b,0x47,0x7e,0x7f,0x6f,
          0x52,0x53,0x50,0x4c,0x4d,0x48,0x01,0x45,0x57,0x4e,0x51,0x4a,0x37,0x49,0x46,0x54,
          0x80,0x81,0x82,0x41,0x54,0x85,0x86,0x87,0x88,0x89,0x8a,0x8b,0x8c,0x8d,0x8e,0x8f,
          0x90,0x91,0x92,0x93,0x94,0x95,0x96,0x97,0x98,0x99,0x9a,0x9b,0x9c,0x9d,0x9e,0x9f,
          0xa0,0xa1,0xa2,0xa3,0xa4,0xa5,0xa6,0xa7,0xa8,0xa9,0xaa,0xab,0xac,0xad,0xae,0xaf,
          0xb0,0xb1,0xb2,0xb3,0xb4,0xb5,0xb6,0xb7,0xb8,0xb9,0xba,0xbb,0xbc,0xbd,0xbe,0xbf,
          0xc0,0xc1,0xc2,0xc3,0xc4,0xc5,0xc6,0xc7,0xc8,0xc9,0xca,0xcb,0xcc,0xcd,0xce,0xcf,
          0xd0,0xd1,0xd2,0xd3,0xd4,0xd5,0xd6,0xd7,0xd8,0xd9,0xda,0xdb,0xdc,0xdd,0xde,0xdf,
          0xe0,0xe1,0xe2,0xe3,0xe4,0xe5,0xe6,0xe7,0xe8,0xe9,0xea,0xeb,0xec,0xed,0xee,0xef,
          0xf0,0xf1,0xf2,0xf3,0xf4,0xf5,0xf6,0xf7,0xf8,0xf9,0xfa,0xfb,0xfc,0xfd,0xfe,0xff
          };
    }

}