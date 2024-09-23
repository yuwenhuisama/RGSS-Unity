# This script is the entry of the RGSS3 scripts.
# It loads the RMVA scripts and runs the main loop.
# It also catches unhandled exceptions and notifies the native side.
# It requires the RMVA scripts and the RMVA scripts require the RGSS3 scripts.

$rmva_project_base_path = Unity.rmva_project_path
$rgss_stop_flag = false
$rtp_path = Unity.rtp_path

def format_exc_string(e)
  exc_str = "Exception: #{e.message} \n"
  exc_str += "Backtrace: \n"
  e.backtrace.each do |line|
    exc_str += "\t" + line + "\n"
  end

  exc_str += "\n"

  # Extracting the script name and line number from the first line of the backtrace
  if e.backtrace && !e.backtrace.empty?
    first_line = e.backtrace.first
    script_name, line_number = first_line.split(":")[0..1]
    exc_str += "\tScript name: #{script_name} \n"
    exc_str += "\tLine number: #{line_number} \n"
  end

  exc_str
end

def read_scripts_and_run(file_path)
  File.open(file_path, 'rb') do |f|
    scripts = Marshal.load(f)
    scripts.each do |script|
      script_id, script_name, script_content = script
      Unity.register_rmva_script(script_id, script_name, script_content)
    end
  end
  main
end

begin
  require 'kernel'
  require 'graphics'
  require 'sprite'
  require 'bitmap'
  require 'viewport'
  require 'plane'
  require 'window'
  require 'input'
  require 'audio'

  sprite_file = File.join($rmva_project_base_path, "RMProject/Data/Scripts.rvdata2")
  read_scripts_and_run sprite_file

rescue Exception => e
  str = format_exc_string(e)
  Unity.on_top_exception(str)
end
