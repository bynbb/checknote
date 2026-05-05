CdevLayerInfo = provider(
    doc = "Clean Architecture boundary metadata for Checknote source groups.",
    fields = {
        "area": "Source area. Expected values: common, module.",
        "module": "Module name for module-scoped targets, or empty for common targets.",
        "layer": "Clean Architecture layer name.",
        "role": "Specialized role inside a layer. Expected values: layer, tokens.",
    },
)

_AREAS = ["common", "module"]
_LAYERS = ["domain", "application", "infrastructure", "presentation", "composition"]
_ROLES = ["layer", "tokens"]

def _cdev_layer_impl(ctx):
    _validate_current_target(ctx)

    direct_files = ctx.files.srcs + ctx.files.assets
    transitive_files = []

    for dep in ctx.attr.deps:
        dep_info = dep[CdevLayerInfo]
        _validate_dependency(ctx, dep.label, dep_info)
        transitive_files.append(dep[DefaultInfo].files)

    return [
        DefaultInfo(files = depset(direct = direct_files, transitive = transitive_files)),
        CdevLayerInfo(
            area = ctx.attr.area,
            module = ctx.attr.module,
            layer = ctx.attr.layer,
            role = ctx.attr.role,
        ),
    ]

def _validate_current_target(ctx):
    if ctx.attr.area not in _AREAS:
        fail("{} has invalid area '{}'. Expected one of: {}".format(ctx.label, ctx.attr.area, _AREAS))

    if ctx.attr.layer not in _LAYERS:
        fail("{} has invalid layer '{}'. Expected one of: {}".format(ctx.label, ctx.attr.layer, _LAYERS))

    if ctx.attr.role not in _ROLES:
        fail("{} has invalid role '{}'. Expected one of: {}".format(ctx.label, ctx.attr.role, _ROLES))

    if ctx.attr.role == "tokens" and ctx.attr.layer != "composition":
        fail("{} uses role 'tokens', which is allowed only in the composition layer.".format(ctx.label))

    if ctx.attr.area == "module" and not ctx.attr.module:
        fail("{} must set module when area is 'module'.".format(ctx.label))

    if ctx.attr.area == "common" and ctx.attr.module:
        fail("{} must not set module when area is 'common'.".format(ctx.label))

def _validate_dependency(ctx, dep_label, dep_info):
    if ctx.attr.role == "tokens":
        _validate_token_dependency(ctx, dep_label, dep_info)
        return

    if ctx.attr.area == "common":
        if dep_info.area != "common":
            _reject(ctx, dep_label, dep_info, "common targets may not depend on module targets")

        _validate_layer_dependency(ctx, dep_label, dep_info, _common_allowed_layers(ctx.attr.layer))
        return

    if dep_info.area == "common":
        _validate_layer_dependency(ctx, dep_label, dep_info, _common_allowed_layers(ctx.attr.layer))
        return

    if dep_info.module != ctx.attr.module:
        _reject(ctx, dep_label, dep_info, "cross-module Bazel dependencies are not allowed")

    _validate_module_layer_dependency(ctx, dep_label, dep_info)

def _validate_token_dependency(ctx, dep_label, dep_info):
    if dep_info.area == "module" and dep_info.module != ctx.attr.module:
        _reject(ctx, dep_label, dep_info, "composition token targets may not depend across modules")

    if dep_info.layer not in ["domain", "application"]:
        _reject(ctx, dep_label, dep_info, "composition token targets may depend only on domain or application targets")

def _validate_module_layer_dependency(ctx, dep_label, dep_info):
    if ctx.attr.layer == "domain":
        allowed_layers = ["domain"]
    elif ctx.attr.layer == "application":
        allowed_layers = ["domain", "application"]
    elif ctx.attr.layer == "infrastructure":
        allowed_layers = ["domain", "application", "infrastructure"]
    elif ctx.attr.layer == "presentation":
        if dep_info.layer == "composition" and dep_info.role == "tokens":
            return
        allowed_layers = ["domain", "application", "presentation"]
    elif ctx.attr.layer == "composition":
        if dep_info.layer == "composition" and dep_info.role != "tokens":
            _reject(ctx, dep_label, dep_info, "composition targets may depend only on composition token targets")
        allowed_layers = ["domain", "application", "infrastructure", "presentation", "composition"]
    else:
        allowed_layers = []

    _validate_layer_dependency(ctx, dep_label, dep_info, allowed_layers)

def _validate_layer_dependency(ctx, dep_label, dep_info, allowed_layers):
    if dep_info.layer not in allowed_layers:
        _reject(
            ctx,
            dep_label,
            dep_info,
            "{} may depend only on layers: {}".format(ctx.attr.layer, allowed_layers),
        )

def _common_allowed_layers(from_layer):
    if from_layer == "domain":
        return ["domain"]

    if from_layer == "application":
        return ["domain", "application"]

    if from_layer == "infrastructure":
        return ["domain", "application", "infrastructure"]

    if from_layer == "presentation":
        return ["domain", "application", "presentation", "composition"]

    if from_layer == "composition":
        return ["domain", "application", "infrastructure", "presentation", "composition"]

    return []

def _reject(ctx, dep_label, dep_info, reason):
    fail("{} ({}/{}/{}) may not depend on {} ({}/{}/{}): {}".format(
        ctx.label,
        ctx.attr.area,
        ctx.attr.module or "common",
        ctx.attr.layer,
        dep_label,
        dep_info.area,
        dep_info.module or "common",
        dep_info.layer,
        reason,
    ))

cdev_layer = rule(
    implementation = _cdev_layer_impl,
    attrs = {
        "area": attr.string(mandatory = True),
        "module": attr.string(default = ""),
        "layer": attr.string(mandatory = True),
        "role": attr.string(default = "layer"),
        "srcs": attr.label_list(allow_files = True),
        "assets": attr.label_list(allow_files = True),
        "deps": attr.label_list(providers = [CdevLayerInfo]),
    },
    doc = "Groups source files and validates declared Clean Architecture layer dependencies.",
)
