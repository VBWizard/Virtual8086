# Virtual80x86

*A data-driven, C#-based x86 **hobby** emulator for DOS, Linux, and beyond.*  
*Real, Protected, and Virtual 8086 modes. With devices. With paging. With insight.*

Written between 2010 and 2016. Tinkered with ever since. Only uploaded to Github in 2025

---

## 🧠 Why This Exists

> *“I looked at the Bochs source once... and decided I never wanted to do that again.”*  
> — Chris (a.k.a. VBWizard)

Most x86 emulators bury their logic under macros and bit-parsing spaghetti.  
**Virtual80x86** is different.

It was built from the ground up in **C#**, designed to be **clear, inspectable, and extensible**.  
Every instruction is decoded through a clean, XML-driven system.  
Every device is modeled in structured, testable, object-oriented code.  
And every clock tick is a moment you can *see.*

---

## ⚙️ Features

- ✅ **Full x86 instruction decoding**  
  - XML-defined opcodes  
  - Operand-aware parsing  
  - Clean C# execution classes

- ✅ **Instruction set coverage**  
  - 8086 / 80186 / 80286 / 80386 / 80486 / Pentium-era support  
  - Progressive FPU coverage (missing many ops, but runtime-completable)

- ✅ **Real/Protected/Virtual 8086 mode support**

- ✅ **Full paging with live TLB tracking**

- ✅ **DOS support**
  - Boots directly to MS-DOS
  - EMM386 confirmed working
  - Real BIOS, hard drive (image) and floppy boot support

- ✅ **Linux support**
  - Boots 1.x-era Linux distros
  - Runs `bash`, `ps`, and multitasking loops
  - Live task name detection via TR-based reverse walk

- ✅ **Custom BIOS + CMOS support**
  - Custom CMOS image generator included
  - Configurable boot devices

- ✅ **Memory Forensics**
  - Memory usage heatmap
  - Real-time paging access overlay
  - CR3-aware task memory analysis

- ✅ **GDT viewer**
  - All descriptors visualized live
  - Ring-level coloring
  - Task awareness

- ✅ **Process Overlay**
  - Current task shown by name (e.g. `bash`, `ps`, `swapper`, etc.)
  - Ring detection
  - Updates per tick

---

## 🧪 What’s Experimental

- ⚠️ **FPU** is partially implemented (many instructions missing)
- ⚠️ Some devices have diagnostic scaffolding but aren’t fully connected
- ⚠️ Many variables and classes will raise compiler warnings (by design or stubbing)

---

## 📁 Project Structure

- `Virtual80x86/`  
  - Core CPU emulation, memory, instruction decoding  
  - Includes all devices (CMOS, PIT, HDC, Floppy, PS2, Serial, etc.)

- `WindowsFormsApplication1/`  
  - The front-end UI for emulator control, visualization, and rendering

- `CreateCMOSImage/`, `Create XML From OpCode List (CSV)/`  
  - Tools for support, boot prep, and instruction list maintenance

---

## 💬 From the Author

> *“When I got interested in emulators, I looked at the BOCHS source code…  
> And thought, ‘There has to be a better way.’”*

This is the result of that thought.

If you’re a systems geek, a reverse engineer, or someone who just wants to see a machine **breathe**, I hope this gives you joy.

Pull it. Explore it. Break it. Rebuild it.

Or just open the live memory viewer and marvel at the color of memory.

— Chris (a.k.a. VBWizard)

---

## 📜 License

TBD by Chris. (Currently private / shared by permission)

---