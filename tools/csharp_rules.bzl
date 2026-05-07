load("@rules_dotnet//dotnet:defs.bzl", _csharp_binary = "csharp_binary", _csharp_library = "csharp_library")

_ANALYZER_CONFIGS = ["//:.editorconfig"]
_ANALYZER_DEPS = ["@paket.main//sonaranalyzer.csharp"]

def checknote_csharp_library(
        name,
        deps = [],
        analyzer_configs = [],
        nullable = "enable",
        treat_warnings_as_errors = True,
        **kwargs):
    _csharp_library(
        name = name,
        deps = deps + _ANALYZER_DEPS,
        analyzer_configs = analyzer_configs + _ANALYZER_CONFIGS,
        nullable = nullable,
        treat_warnings_as_errors = treat_warnings_as_errors,
        **kwargs
    )

def checknote_csharp_binary(
        name,
        deps = [],
        analyzer_configs = [],
        nullable = "enable",
        treat_warnings_as_errors = True,
        **kwargs):
    _csharp_binary(
        name = name,
        deps = deps + _ANALYZER_DEPS,
        analyzer_configs = analyzer_configs + _ANALYZER_CONFIGS,
        nullable = nullable,
        treat_warnings_as_errors = treat_warnings_as_errors,
        **kwargs
    )
