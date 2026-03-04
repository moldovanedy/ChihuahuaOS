import os;
import configparser
import shutil
import subprocess

import bootloader

search_string = "<ProjectReference Include=\""

def get_all_src_files(csproj_path: str) -> list[str]:
    """
    Returns all source files from the given project (.csproj file) and its references.
    """

    csproj_path = csproj_path.replace("\\", "/")
    proj_dir_path = os.path.dirname(csproj_path)
    proj_dir_path += "/"

    cs_files = []
    for src_file in enumerate_source_files(proj_dir_path):
        cs_files.append(proj_dir_path + src_file)
    
    with open(csproj_path, "r", encoding="utf-8") as file:

        for line_number, line in enumerate(file, start=1):
            start_index = line.find(search_string)

            while start_index != -1:
                # start of actual path (after Include=")
                path_start = start_index + len(search_string)

                # find closing quote
                path_end = line.find('"', path_start)

                if path_end != -1:
                    path = line[path_start:path_end]
                    path = path.replace("\\", "/")
                    path = os.path.dirname(path)
                    path += "/"

                    print(f"Found project reference with path: {path}")
                    for src_file in enumerate_source_files(proj_dir_path + path):
                        cs_files.append(proj_dir_path + path + src_file)

                    # continue searching in the same line
                    start_index = line.find(search_string, path_end)
                else:
                    break
    
    return cs_files

def enumerate_source_files(proj_path: str) -> list[str]:
    """
    Recursively finds all .cs files under proj_path,
    excluding /bin and /obj directories.
    
    Returns:
        List[str]: relative file paths
    """

    cs_files = []

    for current_root, dirs, files in os.walk(proj_path):
        # Remove 'bin' and 'obj' from traversal (modifies dirs in-place)
        dirs[:] = [d for d in dirs if d not in ("bin", "obj")]

        for file in files:
            if file.endswith(".cs"):
                full_path = os.path.join(current_root, file)
                relative_path = os.path.relpath(full_path, proj_path)
                cs_files.append(relative_path)

    return cs_files


projects = [bootloader.BootloaderBuilder()]

def build_all():
    host_config = configparser.ConfigParser()
    host_config.read("host_config.ini")
    compiler_path = host_config['host.tools']['bflat_compiler']

    for proj in projects:
        print(f"=== Building {proj.get_proj_name()} ===")
        dist_dir = proj.get_dist_path()
        src_files = get_all_src_files(proj.get_proj_dir() + "ChihuahuaOS.Bootloader.csproj")

        if not os.path.isdir(dist_dir):
            os.makedirs(dist_dir)

        compiler = [
            compiler_path, 
            "build"] + src_files + proj.get_compiler_options(True) + [
            "-o", dist_dir + proj.get_dist_file_name()]
        subprocess.call(compiler)
        print(f"=== Done building {proj.get_proj_name()} ===")
    
    print("~~~ All projects are built ~~~")

def install_all():
    if not os.path.isdir("bin"):
        os.makedirs("bin")

    for proj in projects:
        print(f"=== Installing {proj.get_proj_name()} ===")

        for current_root, dirs, files in os.walk(proj.get_dist_path()):
            for file in files:
                full_path = os.path.join(current_root, file)
                relative_path = os.path.relpath(full_path, proj.get_dist_path())

                final_dir = proj.get_final_publish_path_for_file(relative_path)
                if not os.path.isdir(final_dir):
                    os.makedirs(final_dir)

                shutil.copy2(proj.get_dist_path() + relative_path, final_dir + relative_path)

        print(f"=== Done installing {proj.get_proj_name()} ===")

    print("~~~ All projects are installed ~~~")

if __name__ == "__main__":
    build_all()
    install_all()