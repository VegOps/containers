from importlib.metadata import PackageNotFoundError, version


class DistributionNotFound(Exception):
    pass


class _Distribution:
    def __init__(self, project_name):
        self.project_name = project_name
        try:
            self.version = version(project_name)
        except PackageNotFoundError as exc:
            raise DistributionNotFound(str(exc)) from exc


def get_distribution(project_name):
    return _Distribution(project_name)


def iter_entry_points(*args, **kwargs):
    return iter(())