# Struggles

## Windows 2000 SP4
UHCI is the only way to get a working keyboard & mouse in the BIOS & setup (and possibly the entire OS as well).

With Q35, Win2K crashes with KMODE_EXCEPTION_NOT_HANDLED in USBD.SYS. ACPI doesn't seem to work either.

With i440FX:
- Intel PIIX IDE doesn't work. It'll crash with INACCESSIBLE_BOOT_DEVICE.
- SCSI with LSI53C895A works (other SCSI controllers aren't recognized by SeaBIOS), but it requires external drivers (e.g. via floppy). However, it's particularly slow at the configuration setup stage.
- UniATA doesn't work, and the modded AHCI drivers by Fernando don't either (the setup says iaStor.sys is corrupted).

## Windows NT 4/3.51
Crashes early in setup with a KVM internal error (suberror 3).

## Windows 98 SE
## Windows 95 OSR2.5
