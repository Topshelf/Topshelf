node 'mono.local' {
  $sentinel = "/var/lib/apt/first-puppet-run"
  exec { "apt-get update":
    command => "/usr/bin/apt-get update && touch ${sentinel}",
    onlyif => "/usr/bin/env test \\! -f ${sentinel} || /usr/bin/env test \\! -z \"$(find /etc/apt -type f -c
newer ${sentinel})\"",
    timeout => 3600,
    notify => Exec['apt-get upgrade']
  }
  exec { "apt-get upgrade":
    command => "/usr/bin/apt-get upgrade -y",
    refreshonly => true
  }
  package {
    ['mono-runtime', 'mono-4.0-service']:
    ensure  => present,
    require => Exec['apt-get upgrade']
  }
}
