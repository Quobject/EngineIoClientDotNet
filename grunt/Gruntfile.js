
module.exports = function (grunt) {
  var
    node_os = require('os'),
    fs = require('fs'),
    strip_json = require('strip-json-comments'),
    config = JSON.parse(strip_json(String(fs.readFileSync('./config.json')))),
    util = require('util'),
    os = node_os.platform() === 'win32' ? 'win' : 'linux',
    nuget_builds = [
      { "Name": "EngineIoClientDotNet.net35", "NuGetDir": "net35" },
      { "Name": "EngineIoClientDotNet.net40", "NuGetDir": "net40" },
      { "Name": "EngineIoClientDotNet.net45", "NuGetDir": "net45" },
      { "Name": "EngineIoClientDotNet.windowsphone8", "NuGetDir": "windowsphone8" },
      { "Name": "EngineIoClientDotNet.netcore45", "NuGetDir": "netcore45" },
      { "Name": "EngineIoClientDotNet.portable-wpa81+wp81", "NuGetDir": "portable-wpa81+wp81" },
      { "Name": "EngineIoClientDotNet.portable-win81+wpa81", "NuGetDir": "portable-win81+wpa81", copyOnly: true }
    ];

  grunt.log.writeln(util.inspect(config));
  grunt.log.writeln( 'os = "%s"', os );

  grunt.loadTasks('./tasks');

  grunt.initConfig({      
    os: os,
    config: config,
    //msbuild_configuration: 'Debug',
    msbuild_configuration: 'Release',
    nuget_builds: nuget_builds,
    release_path: './../Releases/<%= config.version %>/',
    working_path: './../Working/',
    server_path: '../TestServer/',
    shell: {
      exec: {
        options: {
          stdout: true,
          stderr: true
        }
      }
    },
    jshint: {
      options: {
        jshintrc: true,
      },
      target: ['Gruntfile.js', '<%= server_path %>server.js', 'tasks/**/*.js']
    },
    clean: {
      release: ['<%= release_path %>/*'],
      working: ['<%= working_path %>/*'],
      options: { force: true }                
    },  
    copy: {
      release: {
        files: [
          {
            expand: true,
            cwd:  './../EngineIoClientDotNet/bin/Release',
            src:  '*',
            dest: '<%= release_path %>/net45'
          }
        ]
      },
      release_mono: {
        files: [
          {
            expand: true,
            cwd:  './../EngineIoClientDotNet_Mono/bin/Release',
            src:  '*',
            dest: '<%= release_path %>/mono'
          }
        ]
      },
    }
  });

  grunt.loadNpmTasks('grunt-contrib-copy');
  grunt.loadNpmTasks('grunt-contrib-clean');  
  grunt.loadNpmTasks('grunt-shell');
  grunt.loadNpmTasks('grunt-contrib-jshint');
  grunt.registerTask('default', ['jshint', 'installNpm', 'nuget', 'buildClient', 'buildTest', 'startServer', 'testClient']);
  grunt.registerTask('test', ['jshint', 'buildClient', 'buildTest', 'testClient']);
  grunt.registerTask('makeNuget', ['jshint','clean:working','createNugetPackage']);
};
