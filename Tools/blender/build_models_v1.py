"""Dusk Garden model family v1 (docs/06 Phase 2.5-A6, shape grammar docs/11 §2.1).

Builds the 14 current modules and 7 current enemies as low-poly meshes and exports one FBX per model to
Assets/Content/Resources/Models/{Modules,Enemies}/<Name>.fbx. Run inside Blender (5.x):

    exec(open(r"<repo>/Tools/blender/build_models_v1.py").read())

Conventions
- 1 unit = 1 m. Blender Z-up; the FBX export converts to Unity Y-up.
- Modules: pivot at the base centre (they rest on a petal). "Outward" (away from the Core) is +Y in Blender.
- Enemies: pivot at the centre of mass; "forward" (towards the Core) is +Y in Blender.
- Material slot 0 = body (the family's three-surface material in game); slot 1 = accent (glowing inside), optional.
- Family silhouettes: Attack = pointed crystals, Boost = rings/discs, Support = solid rounded, Enemy = angular shards.
"""
import math
import os

import bmesh
import bpy
from mathutils import Matrix, Vector

REPO = os.environ.get("TD_REPO", r"C:\Users\edpo\Desktop\Progetti\Giochi\TowerDefense")
OUT = os.path.join(REPO, "Assets", "Content", "Resources", "Models")

HEX = {
    "teal": "5CC8C0", "gold": "E9B872", "mint": "9AD9A1", "coral": "F07A6A", "rose": "E8709A",
    "glow": "FFD9A8", "ivory": "F3E9D7",
}


def srgb_to_linear(c):
    return c / 12.92 if c <= 0.04045 else ((c + 0.055) / 1.055) ** 2.4


def material(name, hex_colour, emission=0.0):
    mat = bpy.data.materials.get(name)
    if mat is None:
        mat = bpy.data.materials.new(name)
        mat.use_nodes = True
    h = HEX[hex_colour]
    rgb = [srgb_to_linear(int(h[i:i + 2], 16) / 255) for i in (0, 2, 4)]
    bsdf = next(n for n in mat.node_tree.nodes if n.type == "BSDF_PRINCIPLED")
    bsdf.inputs["Base Color"].default_value = (*rgb, 1)
    bsdf.inputs["Roughness"].default_value = 0.45
    if "Emission Color" in bsdf.inputs:
        bsdf.inputs["Emission Color"].default_value = (*rgb, 1)
        bsdf.inputs["Emission Strength"].default_value = emission
    mat.diffuse_color = (*rgb, 1)
    return mat


# ---------------------------------------------------------------- geometry helpers (all write into one bmesh)

def add_face(bm, verts, mat_index=0):
    face = bm.faces.new(verts)
    face.material_index = mat_index
    return face


def bipyramid(bm, sides, radius, girdle_z, top_z, bottom_z, centre=(0.0, 0.0), rot=0.0, stretch=(1.0, 1.0), mat_index=0, matrix=None):
    """A gem: a ring of `sides` vertices at girdle height with a point above and below."""
    cx, cy = centre
    ring = []
    for i in range(sides):
        a = 2 * math.pi * i / sides + rot
        ring.append(Vector((cx + math.cos(a) * radius * stretch[0], cy + math.sin(a) * radius * stretch[1], girdle_z)))
    top = Vector((cx, cy, top_z))
    bottom = Vector((cx, cy, bottom_z))
    pts = ring + [top, bottom]
    if matrix is not None:
        pts = [matrix @ p for p in pts]
    vs = [bm.verts.new(p) for p in pts]
    n = sides
    for i in range(n):
        a, b = vs[i], vs[(i + 1) % n]
        add_face(bm, (a, b, vs[n]), mat_index)
        add_face(bm, (b, a, vs[n + 1]), mat_index)


def prism(bm, sides, radius, z0, z1, centre=(0.0, 0.0), rot=0.0, top_scale=1.0, mat_index=0):
    """A capped prism or frustum (flat top)."""
    cx, cy = centre
    low, high = [], []
    for i in range(sides):
        a = 2 * math.pi * i / sides + rot
        low.append(bm.verts.new((cx + math.cos(a) * radius, cy + math.sin(a) * radius, z0)))
        high.append(bm.verts.new((cx + math.cos(a) * radius * top_scale, cy + math.sin(a) * radius * top_scale, z1)))
    for i in range(sides):
        j = (i + 1) % sides
        add_face(bm, (low[i], low[j], high[j], high[i]), mat_index)
    add_face(bm, list(reversed(low)), mat_index)
    add_face(bm, high, mat_index)


def torus(bm, major, minor, z, tilt=0.0, segments=36, sides=10, centre=(0.0, 0.0), mat_index=0):
    cx, cy = centre
    rotation = Matrix.Rotation(tilt, 4, "X")
    grid = []
    for i in range(segments):
        u = 2 * math.pi * i / segments
        row = []
        for j in range(sides):
            v = 2 * math.pi * j / sides
            p = Vector(((major + minor * math.cos(v)) * math.cos(u), (major + minor * math.cos(v)) * math.sin(u), minor * math.sin(v)))
            p = rotation @ p
            row.append(bm.verts.new((p.x + cx, p.y + cy, p.z + z)))
        grid.append(row)
    for i in range(segments):
        for j in range(sides):
            a, b = grid[i][j], grid[(i + 1) % segments][j]
            c, d = grid[(i + 1) % segments][(j + 1) % sides], grid[i][(j + 1) % sides]
            add_face(bm, (a, b, c, d), mat_index)


def sphere(bm, radius, centre, scale=(1.0, 1.0, 1.0), segments=24, rings=12, mat_index=0):
    result = bmesh.ops.create_uvsphere(bm, u_segments=segments, v_segments=rings, radius=radius)
    m = Matrix.Translation(Vector(centre)) @ Matrix.Diagonal((*scale, 1.0))
    bmesh.ops.transform(bm, matrix=m, verts=result["verts"])
    for f in {f for v in result["verts"] for f in v.link_faces}:
        f.material_index = mat_index


def cylinder(bm, radius, z0, z1, segments=28, mat_index=0):
    prism(bm, segments, radius, z0, z1, mat_index=mat_index)


def box(bm, size, centre, mat_index=0):
    result = bmesh.ops.create_cube(bm, size=1.0)
    m = Matrix.Translation(Vector(centre)) @ Matrix.Diagonal((*size, 1.0))
    bmesh.ops.transform(bm, matrix=m, verts=result["verts"])
    for f in {f for v in result["verts"] for f in v.link_faces}:
        f.material_index = mat_index


# ---------------------------------------------------------------- the family

def m_emitter(bm):
    bipyramid(bm, 6, 0.22, 0.24, 0.9, 0.0)


def m_scatter(bm):
    for k in range(3):
        a = 2 * math.pi * k / 3 + math.pi / 2
        bipyramid(bm, 5, 0.12, 0.14, 0.55 + 0.08 * (k == 0), 0.0, centre=(math.cos(a) * 0.16, math.sin(a) * 0.16), rot=k)


def m_arc(bm):
    bipyramid(bm, 6, 0.18, 0.2, 0.8, 0.0)
    torus(bm, 0.3, 0.025, 0.42, tilt=math.radians(24), segments=40, sides=6, mat_index=1)


def m_lance(bm):
    # A long crystal lying flat, pointing outward (+Y).
    m = Matrix.Translation((0, 0, 0.22)) @ Matrix.Rotation(math.radians(-90), 4, "X")
    bipyramid(bm, 4, 0.14, 0.0, 0.62, -0.38, matrix=m, rot=math.pi / 4)


def m_mortar(bm):
    bipyramid(bm, 8, 0.34, 0.22, 0.34, 0.0, rot=math.pi / 8)
    prism(bm, 8, 0.2, 0.3, 0.46, rot=math.pi / 8, top_scale=0.7)


def m_amplifier(bm):
    torus(bm, 0.27, 0.085, 0.24, tilt=math.radians(20))


def m_lens(bm):
    sphere(bm, 0.3, (0, 0, 0.22), scale=(1.0, 1.0, 0.32), segments=28, rings=10)


def m_overclock(bm):
    torus(bm, 0.24, 0.07, 0.22, tilt=math.radians(20))
    tilt = Matrix.Rotation(math.radians(20), 4, "X")
    for k in range(8):
        a = 2 * math.pi * k / 8
        p = tilt @ Vector((math.cos(a) * 0.34, math.sin(a) * 0.34, 0))
        box(bm, (0.08, 0.08, 0.08), (p.x, p.y, p.z + 0.22))


def m_echo(bm):
    torus(bm, 0.3, 0.05, 0.24, tilt=math.radians(20))
    torus(bm, 0.16, 0.045, 0.24, tilt=math.radians(20), mat_index=1)


def m_bank(bm):
    for k in range(3):
        prism(bm, 28, 0.26, 0.02 + k * 0.11, 0.11 + k * 0.11, top_scale=0.97)


def m_salvage(bm):
    box(bm, (0.46, 0.36, 0.3), (0, 0, 0.16))
    box(bm, (0.5, 0.4, 0.07), (0, 0, 0.33))


def m_bulwark(bm):
    sphere(bm, 0.32, (0, 0, 0.02), scale=(1.0, 1.0, 0.75), segments=28, rings=14)
    cylinder(bm, 0.34, 0.0, 0.05)


def m_frost(bm):
    sphere(bm, 0.2, (0, 0, 0.2), segments=20, rings=10)
    bipyramid(bm, 12, 0.17, 0.26, 0.62, 0.26)


def m_capacitor(bm):
    for k in range(3):
        prism(bm, 24, 0.23 - k * 0.02, 0.02 + k * 0.13, 0.12 + k * 0.13, top_scale=0.96)


def e_drifter(bm):
    m = Matrix.Rotation(math.radians(-90), 4, "X")  # long axis along +Y (towards the Core)
    bipyramid(bm, 4, 0.12, 0.0, 0.42, -0.28, matrix=m, rot=math.pi / 4)


def e_swarmlet(bm):
    s = 0.13
    v = [bm.verts.new(p) for p in ((0, s * 1.2, -s * 0.3), (-s, -s * 0.6, -s * 0.3), (s, -s * 0.6, -s * 0.3), (0, 0, s * 0.8))]
    add_face(bm, (v[0], v[2], v[1]))
    add_face(bm, (v[0], v[1], v[3]))
    add_face(bm, (v[1], v[2], v[3]))
    add_face(bm, (v[2], v[0], v[3]))


def e_brute(bm):
    prism(bm, 6, 0.3, -0.16, 0.16, top_scale=0.86)


def e_dasher(bm):
    m = Matrix.Rotation(math.radians(-90), 4, "X")
    bipyramid(bm, 3, 0.11, 0.0, 0.36, -0.22, matrix=m, rot=math.pi / 2)
    for side in (-1, 1):
        v = [bm.verts.new(p) for p in ((0, -0.1, 0), (side * 0.18, -0.3, 0), (0, -0.22, 0.04))]
        add_face(bm, v if side > 0 else list(reversed(v)))


def e_splitter(bm):
    m = Matrix.Rotation(math.radians(-90), 4, "X")
    bipyramid(bm, 5, 0.16, 0.0, 0.34, -0.26, matrix=m)
    box(bm, (0.035, 0.34, 0.2), (0, 0.02, 0.0), mat_index=1)  # the glowing crack


def e_warden(bm):
    m = Matrix.Rotation(math.radians(-90), 4, "X")
    bipyramid(bm, 6, 0.17, 0.0, 0.36, -0.26, matrix=m)
    prism(bm, 6, 0.08, 0.12, 0.22, top_scale=0.4, mat_index=1)


def e_guardian(bm):
    bipyramid(bm, 6, 0.42, 0.0, 0.85, -0.55)
    for k in range(3):
        a = 2 * math.pi * k / 3
        bipyramid(bm, 4, 0.07, 0.0, 0.18, -0.12, centre=(math.cos(a) * 0.75, math.sin(a) * 0.75), rot=a)


MODULES = {
    "Emitter": (m_emitter, "teal"), "Scatter": (m_scatter, "teal"), "Arc": (m_arc, "teal"), "Lance": (m_lance, "teal"),
    "Mortar": (m_mortar, "teal"), "Amplifier": (m_amplifier, "gold"), "Lens": (m_lens, "gold"), "Overclock": (m_overclock, "gold"),
    "Echo": (m_echo, "gold"), "Bank": (m_bank, "mint"), "Salvage": (m_salvage, "mint"), "Bulwark": (m_bulwark, "mint"),
    "Frost": (m_frost, "mint"), "Capacitor": (m_capacitor, "mint"),
}
ENEMIES = {
    "Drifter": (e_drifter, "coral"), "Swarmlet": (e_swarmlet, "coral"), "Brute": (e_brute, "coral"), "Dasher": (e_dasher, "coral"),
    "Splitter": (e_splitter, "coral"), "Warden": (e_warden, "coral"), "Guardian": (e_guardian, "rose"),
}
SMOOTH = {"Lens", "Bulwark", "Frost", "Amplifier", "Overclock", "Echo"}


def build(name, fn, colour, collection, x):
    bm = bmesh.new()
    fn(bm)
    bmesh.ops.remove_doubles(bm, verts=bm.verts, dist=1e-5)
    bmesh.ops.recalc_face_normals(bm, faces=bm.faces)
    mesh = bpy.data.meshes.new(name)
    bm.to_mesh(mesh)
    bm.free()
    mesh.materials.append(material(f"Body_{colour}", colour))
    mesh.materials.append(material("Accent_glow", "glow", emission=1.0))
    for poly in mesh.polygons:
        poly.use_smooth = name in SMOOTH
    obj = bpy.data.objects.new(name, mesh)
    obj["td_name"] = name  # the export file name, even if Blender renamed the object (name clash)
    collection.objects.link(obj)
    obj.location = (x, 0, 0)
    return obj


def export(obj, folder):
    """Exports one object through a temporary collection (independent of selection and of the active scene)."""
    os.makedirs(folder, exist_ok=True)
    temp = bpy.data.collections.new("TD_Export")
    bpy.context.scene.collection.children.link(temp)
    location = obj.location.copy()
    obj.location = (0, 0, 0)
    temp.objects.link(obj)
    try:
        bpy.ops.export_scene.fbx(
            filepath=os.path.join(folder, obj["td_name"] + ".fbx"), use_selection=False, collection=temp.name,
            object_types={"MESH"}, apply_unit_scale=True, apply_scale_options="FBX_SCALE_ALL", bake_space_transform=True,
            axis_forward="-Z", axis_up="Y", mesh_smooth_type="FACE", use_mesh_modifiers=True, add_leaf_bones=False,
            bake_anim=False, use_metadata=False)
    finally:
        temp.objects.unlink(obj)
        bpy.data.collections.remove(temp)
        obj.location = location


def main():
    scene = bpy.data.scenes.get("Family") or bpy.data.scenes.new("Family")
    bpy.context.window.scene = scene
    for coll in list(scene.collection.children):
        if coll.name.startswith("Family_"):
            for o in list(coll.objects):
                bpy.data.objects.remove(o, do_unlink=True)
            bpy.data.collections.remove(coll)
    modules = bpy.data.collections.new("Family_Modules")
    enemies = bpy.data.collections.new("Family_Enemies")
    scene.collection.children.link(modules)
    scene.collection.children.link(enemies)
    built = []
    for i, (name, (fn, colour)) in enumerate(MODULES.items()):
        built.append((build(name, fn, colour, modules, i * 1.2), "Modules"))
    for i, (name, (fn, colour)) in enumerate(ENEMIES.items()):
        o = build(name, fn, colour, enemies, i * 1.4)
        o.location.y = -2.5
        built.append((o, "Enemies"))
    for obj, folder in built:
        export(obj, os.path.join(OUT, folder))
    print("exported", len(built), "models to", OUT)


main()
