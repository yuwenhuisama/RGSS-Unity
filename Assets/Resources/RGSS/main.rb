# This script is the entry of the RGSS3 scripts.
# It loads the RMVA scripts and runs the main loop.
# It also catches unhandled exceptions and notifies the native side.
# It requires the RMVA scripts and the RMVA scripts require the RGSS3 scripts.

msgbox "Initialize RGSS3 scripts..."

$rmva_project_base_path = Unity.rmva_project_path
$rgss_stop_flag = false
$rtp_path = Unity.rtp_path

msgbox "RMVA project path: #{$rmva_project_base_path}"
msgbox "RTP path: #{$rtp_path}"

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

def run_scripts
  sprite_file = File.join($rmva_project_base_path, "RMProject/Data/Scripts.rvdata2")
  File.open(sprite_file, 'rb') do |f|
    scripts = Marshal.load(f)
    scripts.each do |script|
      script_id, script_name, script_content = script
      Unity.register_rmva_script(script_id, script_name, script_content)
    end
  end

  Unity.run_rmva_scripts
end

begin
  require 'kernel'
  require 'tone'
  require 'rect'
  require 'color'
  require 'font'
  require 'graphics'
  require 'input'
  require 'audio'
  require 'sprite'
  require 'bitmap'
  require 'viewport'
  require 'plane'
  require 'window'
  require 'table'
  require 'tilemap'
  require 'rgss_error'
  require 'rgss_reset'

  run_scripts
  msgbox "RGSS3 scripts loaded successfully."

  require 'patch_rmva'

  msgbox "Running RGSS3 main loop..."
  $rgss_main_callback.call if $rgss_main_callback
rescue Exception => e
  str = format_exc_string(e)
  Unity.on_top_exception(str)
end
