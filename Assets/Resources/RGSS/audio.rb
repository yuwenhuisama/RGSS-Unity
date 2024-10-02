# encoding: utf-8
require 'type_check_util'

module Audio
  extend TypeCheckUtil

  # Audio play is an asynchronized operation, so we need to set exception handler
  def self.__set_exception_handler__(handler)
    Unity::Audio.set_exception_handler handler
  end
  
  def self.bgm_play(filename, volume = 100, pitch = 100, pos = 0, on_loaded = nil)
    check_arguments([filename, volume, pitch, pos, on_loaded], [String, Integer, Integer, [Float, Integer], [Proc, NilClass]])
    Unity::Audio.bgm_play(filename, volume.clamp(0, 100), pitch.clamp(-300, 300), pos, on_loaded)
  end

  def self.bgm_fade(time)
    check_arguments([time], [Integer])
    Unity::Audio.bgm_fade(time)
  end

  def self.bgs_play(filename, volume = 100, pitch = 100, pos = 0, on_loaded = nil)
    check_arguments([filename, volume, pitch, pos, on_loaded], [String, Integer, Integer, [Float, Integer], [Proc, NilClass]])
    Unity::Audio.bgs_play(filename, volume.clamp(0, 100), pitch.clamp(-300, 300), pos, on_loaded)
  end

  def self.bgs_fade(time)
    check_arguments([time], [Integer])
    Unity::Audio.bgs_fade(time)
  end

  def self.me_play(filename, volume = 100, pitch = 100, on_loaded = nil)
    check_arguments([filename, volume, pitch, on_loaded], [String, Integer, Integer, [Proc, NilClass]])
    Unity::Audio.me_play(filename, volume.clamp(0, 100), pitch.clamp(-300, 300), on_loaded)
  end

  def self.me_fade(time)
    check_arguments([time], [Integer])
    Unity::Audio.me_fade(time)
  end

  def self.se_play(filename, volume = 100, pitch = 100, on_loaded = nil)
    check_arguments([filename, volume, pitch, on_loaded], [String, Integer, Integer, [Proc, NilClass]])
    Unity::Audio.se_play(filename, volume.clamp(0, 100), pitch.clamp(-300, 300), on_loaded)
  end

  [:setup_midi, :bgm_stop, :bgm_pos, :bgs_stop, :bgs_pos, :me_stop, :set_stop].each do |func|
    define_singleton_method(func) do |*args|
      Unity::Audio.send(func, *args)
    end
  end
end
