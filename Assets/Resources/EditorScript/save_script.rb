# encoding: utf-8
# Ruby script Extracts all scripts from the RMVA project to the editor's script folder

scripts_folder = File.join($streaming_assets_base_path, "RMProject", "ExportedScripts")
script_file = File.join($streaming_assets_base_path, "RMProject/Data/Scripts.rvdata2")

begin
  if Dir.exist? scripts_folder
    scripts = []

    Dir.foreach(scripts_folder) do |file|
      next if file == '.' or file == '..'
      next unless file.end_with? '.rb'
      raw_script_file = File.join(scripts_folder, file)
      name_regex = /(.*)-(.*)-(.*)\.rb/
      match_data = file.match(name_regex)

      unless match_data.nil?
        index, script_name, script_id = match_data.captures
        index = index.to_i
        script_id = script_id.to_i
        script_content = File.read(gbk2utf8(raw_script_file))
        deflated = Zlib.deflate(script_content)
        
        script_name = gbk2utf8(script_name)

        script = [index, script_id, script_name, deflated]
        scripts << script
      end
    end

    output = scripts.sort_by! { |s| s[0] }.map { |s| [s[1], s[2], s[3]] }

    if File.exist? gbk2utf8(script_file)
      File.delete gbk2utf8(script_file)
    end

    File.open(gbk2utf8(script_file), 'wb') do |f|
      Marshal.dump(output, f)
    end
  end
rescue Exception => e
  p e.message
  p e.backtrace
end
