require 'type_check_util'

module Audio
  extend TypeCheckUtil

  def self.bgm_play(filename, volume = 100, pitch = 100, pos = 0)
    check_arguments([filename, volume, pitch, pos], [String, Integer, Integer, Integer])
    Unity::Audio.bgm_play(filename, volume, pitch, pos)
  end

  def self.bgm_fade(time)
    check_arguments([time], [Integer])
    Unity::Audio.bgm_fade(time)
  end

  def self.bgs_play(filename, volume = 100, pitch = 100, pos = 0)
    check_arguments([filename, volume, pitch, pos], [String, Integer, Integer, Integer])
    Unity::Audio.bgs_play(filename, volume, pitch, pos)
  end

  def self.bgs_fade(time)
    check_arguments([time], [Integer])
    Unity::Audio.bgs_fade(time)
  end

  def self.me_play(filename, volume = 100, pitch = 100)
    check_arguments([filename, volume, pitch], [String, Integer, Integer])
    Unity::Audio.me_play(filename, volume, pitch, pos)
  end

  def self.me_fade(time)
    check_arguments([time], [Integer])
    Unity::Audio.me_fade(time)
  end

  def self.se_play(filename, volume = 100, pitch = 100)
    check_arguments([filename, volume, pitch, pos], [String, Integer, Integer])
    Unity::Audio.se_play(filename, volume, pitch, pos)
  end

  [:setup_midi, :bgm_stop, :bgm_pos, :bgs_stop, :bgs_pos, :me_stop, :set_stop].each do |func|
    define_singleton_method(func) do |*args|
      Unity::Audio.send(func, *args)
    end
  end
end
