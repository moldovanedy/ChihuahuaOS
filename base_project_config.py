import abc

class BaseBuilder(abc.ABC):
    __metaclass__ = abc.ABCMeta

    @abc.abstractmethod
    def get_proj_dir(self) -> str:
        pass

    @abc.abstractmethod
    def get_proj_name(self) -> str:
        pass
    
    @abc.abstractmethod
    def get_compiler_options(self, is_debug: bool) -> list[str]:
        pass

    @abc.abstractmethod
    def get_dist_path(self) -> str:
        pass

    @abc.abstractmethod
    def get_dist_file_name(self) -> str:
        pass


    @abc.abstractmethod
    def get_final_publish_path_for_file(self, dist_file_path: str) -> str:
        pass