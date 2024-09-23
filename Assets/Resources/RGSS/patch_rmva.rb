module SceneManager
  def self.run
    DataManager.init
    Audio.setup_midi if use_midi?
    @scene = first_scene_class.new
    @scene.main
  end
end

class Scene_Base
  def main
    start
    post_start

    Unity.on_update = proc do
      if $rgss_stop_flag
        Graphics.update
        return
      end

      begin
        patched_scene_update
      rescue Exception => e
        str = format_exc_string(e)
        Unity.on_top_exception(str)
      end
    end
  end

  def patched_scene_update
    if not scene_changing?
      update
    else
      pre_terminate
      terminate

      # switch to new scene
      if SceneManager.scene != nil
        SceneManager.scene.main
      else
        Unity.exit_game
      end
    end
  end
end

class Scene_Title
  alias :old_initialize :initialize
  def initialize
    old_initialize
    @waiting_for_command_window_close = false
  end
  
  alias :old_patched_scene_update :patched_scene_update
  def patched_scene_update
    if @waiting_for_command_window_close and not @command_window.close?
      update
      return
    end
    old_patched_scene_update
  end
  
  def close_command_window
    @command_window.close
    @waiting_for_command_window_close = true
  end
end

class Scene_End
  alias :old_initialize :initialize
  def initialize
    old_initialize
    @waiting_for_command_window_close = false
  end

  alias :old_patched_scene_update :patched_scene_update
  def patched_scene_update
    if @waiting_for_command_window_close and not @command_window.close?
      update
      return
    end
    old_patched_scene_update
  end

  def close_command_window
    @command_window.close
    @waiting_for_command_window_close = true
  end
end

class Scene_Battle
  def main
    @update_fiber = Fiber.new do
      start
      post_start
      until scene_changing?
        update
        Fiber.yield
      end
      pre_terminate
      terminate

      Unity.unregister_update_fiber
    end
    Unity.register_update_fiber @update_fiber
  end
  
  alias :old_update_for_wait :update_for_wait
  def update_for_wait
    old_update_for_wait
    Fiber.yield
  end

  def process_event
    while !scene_changing?
      $game_troop.interpreter.update
      $game_troop.setup_battle_event
      wait_for_message
      wait_for_effect if $game_troop.all_dead?
      process_forced_action
      BattleManager.judge_win_loss
      break unless $game_troop.interpreter.running?
      update_for_wait
      Fiber.yield
    end
  end
end

module DataManager
  def self.__get_real_path__(*path)
    res = File.join($rmva_project_base_path, "RMProject", *path)
    res
  end

  alias :old_save_file_exists? :save_file_exists?
  def self.save_file_exists?
    path = __get_real_path__('Save*.rvdata2')
    old_save_file_exists?(path)
  end

  alias :old_make_filename :make_filename
  def self.make_filename(index)
    filename old_make_filename(index)
    __get_real_path__(filename)
  end
end

module Audio
  alias :old_bgm_play :bgm_play
  def self.bgm_play(filename, volume = 100, pitch = 100, pos = 0)
    path = DataManager.__get_real_path__(filename)
    old_bgm_play(path, volume, pitch, pos)
  end

  alias :old_bgs_play :bgs_play
  def self.bgs_play(filename, volume = 100, pitch = 100, pos = 0)
    path = DataManager.__get_real_path__(filename)
    old_bgs_play(path, volume, pitch, pos)
  end

  alias :old_me_play :me_play
  def self.me_play(filename, volume = 100, pitch = 100)
    path = DataManager.__get_real_path__(filename)
    old_me_play(path, volume, pitch)
  end

  alias :old_se_play :se_play
  def self.se_play(filename, volume = 100, pitch = 100)
    path = DataManager.__get_real_path__(filename)
    old_se_play(path, volume, pitch)
  end
end

module Cache
  alias :old_load_bitmap :load_bitmap
  def self.load_bitmap(folder_name, filename, hue = 0)
    path = DataManager.__get_real_path__(folder_name, filename)
    old_load_bitmap(folder_name, path, hue)
  end
end

Kernel.rgss_main_callback.call if $rgss_main_callback
