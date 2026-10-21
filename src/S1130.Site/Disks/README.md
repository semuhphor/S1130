# Disk library

System disk images (2315 cartridges for the 2310 drive) that the web site can boot.
Each file is one system generation, and its name says how it was genned:

```
<system>-<release>-<printer>-<card device>-<core>[-<extra>].dsk
```

| Part          | Values                                                        |
|---------------|---------------------------------------------------------------|
| `system`      | `dms` (Disk Monitor System), and later other systems          |
| `release`     | `r2v12` = DMS Version 2 Modification 12                       |
| `printer`     | `1403` or `1132`: the principal print device                  |
| `card device` | `1442` (1442 card read punch) or `2501` (2501 card reader)    |
| `core`        | `8k`, `16k` or `32k`: the core size the system was genned for |
| `extra`       | optional notes, e.g. `fortran`, `cobol`, `test`               |

Names are lower case. Never change a disk after it is listed here. If it needs changing, make a new disk with a new name.

The site's master disk is chosen by `Site:MasterDisk` in `appsettings.json`, and files are
looked up in this folder (`Site:DiskLibrary`). Each session boots a private copy, so the
files here are never written.

## Disks

| File                          | System        | Printer | Cards | Core | Cartridge | Contents / how it was built                                      |
|-------------------------------|---------------|---------|-------|------|-----------|------------------------------------------------------------------|
| `dms-r2v12-1403-1442-32k.dsk` | DMS V2 M12    | 1403    | 1442  | 32K  | 000B      | DMS R2V12 rebuilt (`loaddms`) for the 1403, the 1442 and 32K core (core word /0E = /8000). Includes FORTRAN and the assembler. Was `ForReferenceOnly/1130SW/dms.dsk`. The site's default master disk. |
