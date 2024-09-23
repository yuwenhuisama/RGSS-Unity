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

def main
  # Unity.run_scripts
  
  Unity.on_update = proc do
    # to catch unhandled exceptions and notify native side
    begin
      ::Graphics.update
      Input.update
    rescue Exception => e
      str = format_exc_string(e)
      Unity.on_top_exception(str)
    end
  end
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

  base_path = Unity.rmva_project_path
  sprite_file = File.join(base_path, "RMProject/Data/Scripts.rvdata2")
  read_scripts_and_run sprite_file

rescue Exception => e
  str = format_exc_string(e)
  Unity.on_top_exception(str)
end
