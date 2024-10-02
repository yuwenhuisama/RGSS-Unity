add_rules("mode.debug", "mode.releasedbg")

local os_name = os.host()
local mruby_dir = "E:/Projects/mruby-for-dotnet/mruby"
local ext_base_name = "mruby_onig_regexp"
local gem_name = "mruby-onig-regexp"

function common_settings()
    set_arch("x64")
    set_kind("shared")

    add_includedirs(mruby_dir .. "/build/host/include/")
    add_includedirs("Onigmo/")
    add_files(gem_name .. "/src/*.c")

    add_defines("MRB_INT64", "MRB_NO_PRESYM", "MRB_UTF8_STRING")
    add_defines("HAVE_ONIGMO_H")
end

function after_build_macos(target)
    local mode = is_mode("debug") and "debug" or "release"
    local output_dir = path.join(os.projectdir(), string.format("build/macosx/universal/%s/", mode))
    os.exec("mkdir -p %s", output_dir)
    os.exec("lipo -create -output %s %s %s", 
            path.join(output_dir, "lib" .. ext_base_name .. "_ext_x64.dylib"), 
            path.join(os.projectdir(), string.format("build/macosx/arm64/%s/" .. ext_base_name.. "_ext_arm64.dylib", mode)), 
            path.join(os.projectdir(), string.format("build/macosx/x86_64/%s/".. ext_base_name .. "_ext_x86_64.dylib", mode)))
end

target(ext_base_name .. "_ext_x64")
    if os_name == "windows" then
        common_settings()

        add_files("export.def")
        set_basename("lib" .. ext_base_name .. "_ext_x64")
        add_links("lib/libmruby_x64.lib")
        add_links("lib/onigmo_s.lib")
    elseif os_name == "linux" then
        common_settings()

        set_basename(ext_base_name .. "_ext_x64")
        add_links("lib/libmruby_x64.so")
    elseif os_name == "macosx" then
        -- Build for x86_64
        target(ext_base_name .. "_ext_x86_64")
            common_settings()

            set_basename(ext_base_name .. "_ext_x86_64")
            set_arch("x86_64")
            add_links("lib/libmruby_x64.dylib")

        target(ext_base_name .. "_ext_arm64")
            common_settings()

            set_basename(ext_base_name .. "_ext_arm64")
            set_arch("arm64")
            add_links("lib/libmruby_x64.dylib")

            -- Combine into a universal binary
        target(ext_base_name .. "_ext_universal")
            set_kind("phony")
            add_deps(ext_base_name .. "_ext_x86_64", ext_base_name .. "_ext_arm64")
            after_build(after_build_macos)
    else
        -- error: not support platform
        print("unsupported platform")
    end