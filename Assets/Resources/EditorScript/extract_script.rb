# encoding: utf-8
# Ruby script Extracts all scripts from the RMVA project to the editor's script folder

script_file = File.join($streaming_assets_base_path, "RMProject/Data/Scripts.rvdata2")
scripts_folder = File.join($streaming_assets_base_path, "RMProject", "ExportedScripts")

begin
  File.open(script_file, 'rb') do |f|
    scripts = Marshal.load(f)
    scripts.each_with_index do |script, index|
      script_id, script_name, script_content = script

      output_name = "#{index}-#{script_name}-#{script_id}"
      script_content = Zlib::inflate(script_content)

      script_output_name = File.join scripts_folder, "#{output_name}.rb"
      File.open(script_output_name, 'wb') do |f|
        f.write(script_content)
      end
    end
  end
rescue Exception => e
  p e.message
  p e.backtrace
end