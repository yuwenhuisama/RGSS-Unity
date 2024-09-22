add_rules("mode.debug", "mode.releasedbg")

local os_name = os.host()
local mruby_dir = "E:/Projects/mruby-for-dotnet/mruby"

function common_settings()
    set_arch("x64")
    set_kind("shared")

    add_includedirs("mruby-marshal-c/include/")
    add_includedirs(mruby_dir .. "/build/host/include/")
    add_files("mruby-marshal-c/src/*.c")

    add_defines("MRB_INT64", "MRB_NO_PRESYM")
end

function after_build_macos(target)
    local mode = is_mode("debug") and "debug" or "release"
    local output_dir = path.join(os.projectdir(), string.format("build/macosx/universal/%s/", mode))
    os.exec("mkdir -p %s", output_dir)
    os.exec("lipo -create -output %s %s %s", 
            path.join(output_dir, "libmruby_marshal_ext_x64.dylib"), 
            path.join(os.projectdir(), string.format("build/macosx/arm64/%s/mruby_marshal_ext_arm64.dylib", mode)), 
            path.join(os.projectdir(), string.format("build/macosx/x86_64/%s/mruby_marshal_ext_x86_64.dylib", mode)))
end

target("mruby_marshal_ext_x64")
    if os_name == "windows" then
        common_settings()

        add_files("export.def")
        set_basename("libmruby_marshal_ext_x64")
        set_runtimes("MD")
        add_links("Ws2_32.lib", "lib/libmruby_x64.lib")
        add_ldflags("/OPT:REF", "/OPT:ICF")
    elseif os_name == "linux" then
        common_settings()

        set_basename("mruby_marshal_ext_x64")
        add_links("libmruby_x64.so")
    elseif os_name == "macosx" then
        -- Build for x86_64
        target("mruby_marshal_ext_x86_64")
            common_settings()

            set_basename("mruby_marshal_ext_x86_64")
            set_arch("x86_64")
            add_links("lib/libmruby_x64.dylib")

        target("mruby_marshal_ext_arm64")
            common_settings()

            set_basename("mruby_marshal_ext_arm64")
            set_arch("arm64")
            add_links("lib/libmruby_x64.dylib")

                -- Combine into a universal binary
        target("mruby_marshal_ext_universal")
            set_kind("phony")
            add_deps("mruby_marshal_ext_x86_64", "mruby_marshal_ext_arm64")
            after_build(after_build_macos)
    else
        -- error: not support platform
        print("unsupported platform")
    end