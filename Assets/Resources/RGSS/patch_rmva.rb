# encoding: utf-8
module SceneManager
  def self.run
    DataManager.init
    Audio.setup_midi if use_midi?
    @scene = first_scene_class.new

    @update_fiber = Fiber.new do
      begin
        @scene.main while @scene
      rescue Exception => e
        str = format_exc_string(e)
        Unity.on_top_exception(str)
      end
      Unity.unregister_update_fiber
    end

    Unity.register_update_fiber @update_fiber
  end
end

class Scene_Base
  def main
    start
    post_start
    __yield_update until scene_changing?
    pre_terminate
    terminate
  end

  def __yield_update
    if $rgss_stop_flag
      Graphics.update
    else
      update
    end
    Fiber.yield
  end
end

class Scene_Title
  def close_command_window
    @command_window.close
    __yield_update until @command_window.close?
  end
end

class Scene_End
  def close_command_window
    @command_window.close
    __yield_update until @command_window.close?
  end
end

class Scene_Battle
  alias :old_update_for_wait :update_for_wait

  def update_for_wait
    old_update_for_wait
    Fiber.yield
  end

  def process_event
    until scene_changing?
      $game_troop.interpreter.update
      $game_troop.setup_battle_event
      wait_for_message
      wait_for_effect if $game_troop.all_dead?
      process_forced_action
      BattleManager.judge_win_loss
      break unless $game_troop.interpreter.running?
      update_for_wait
    end
  end
end

module DataManager
  def self.__get_real_path__(*path)
    File.join($rmva_project_base_path, "RMProject", *path)
  end

  def self.__get_rtp_path__(*path)
    File.join($rtp_path, *path)
  end

  class << self
    alias :old_save_file_exists? :save_file_exists?

    def save_file_exists?
      path = __get_real_path__('Save*.rvdata2')
      !Dir.glob(path).empty?
    end

    alias :old_make_filename :make_filename

    def make_filename(index)
      filename = old_make_filename(index)
      __get_real_path__(filename)
    end
  end
end

module Audio
  class << self
    alias :old_bgm_play :bgm_play

    def bgm_play(filename, volume = 100, pitch = 100, pos = 0)
      unless filename.include?('\.')
        filename = filename + ".ogg"
      end
      path = DataManager.__get_real_path__(filename)
      old_bgm_play(path, volume, pitch, pos, proc {
        rtp_path = DataManager.__get_rtp_path__(filename)
        old_bgm_play(rtp_path, volume, pitch, pos)
      })
    end

    alias :old_bgs_play :bgs_play

    def bgs_play(filename, volume = 100, pitch = 100, pos = 0)
      unless filename.include?('\.')
        filename = filename + ".ogg"
      end
      path = DataManager.__get_real_path__(filename)
      old_bgs_play(path, volume, pitch, pos, proc {
        rtp_path = DataManager.__get_rtp_path__(filename)
        old_bgs_play(rtp_path, volume, pitch, pos)
      })
    end

    alias :old_me_play :me_play

    def me_play(filename, volume = 100, pitch = 100)
      unless filename.include?('\.')
        filename = filename + ".ogg"
      end
      path = DataManager.__get_real_path__(filename)
      old_me_play(path, volume, pitch, proc {
        rtp_path = DataManager.__get_rtp_path__(filename)
        old_me_play(rtp_path, volume, pitch)
      })
    end

    alias :old_se_play :se_play

    def se_play(filename, volume = 100, pitch = 100)
      unless filename.include?('\.')
        filename = filename + ".ogg"
      end
      path = DataManager.__get_real_path__(filename)
      old_se_play(path, volume, pitch, proc {
        rtp_path = DataManager.__get_rtp_path__(filename)
        old_se_play(rtp_path, volume, pitch)
      })
    end
  end
end

module Cache
  class << self
    alias :old_load_bitmap :load_bitmap

    def load_bitmap(folder_name, filename, hue = 0)
      unless filename.include?('\.')
        filename = filename + ".png"
      end
      path = DataManager.__get_real_path__(folder_name)
      begin
        old_load_bitmap(path, filename, hue)
      rescue
        # try to load bitmap from rtp
        path = DataManager.__get_rtp_path__(folder_name)
        old_load_bitmap(path, filename, hue)
      end
    end
  end
end
