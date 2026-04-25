from base_project_config import BaseBuilder

proj_name = "ChihuahuaOS.Bootloader"

class BootloaderBuilder(BaseBuilder):

    def get_proj_dir(self) -> str:
        return "src/Boot/" + proj_name + "/"

    def get_proj_name(self) -> str:
        return proj_name

    def get_compiler_options(self, is_debug: bool) -> list[str]:
        return [
            "--no-pie", 
            "--separate-symbols", 
            "--stdlib", "none",
            "--os", "uefi"
        ]

    def get_dist_path(self) -> str:
        return "src/Boot/" + proj_name + "/dist/"
    
    def get_dist_file_name(self) -> str:
        return "BOOTX64.EFI"
    

    def get_final_publish_path_for_file(self, dist_file_path: str) -> str:
        return "bin/EFI/BOOT/"
